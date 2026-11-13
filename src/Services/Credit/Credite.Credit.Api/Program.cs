using Confluent.Kafka;
using Credite.Credit.Api.Dados;
using Credite.Credit.Api.Endpoints;
using Credite.Credit.Api.Servicos;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    ContentRootPath = AppContext.BaseDirectory
});

builder.Services.AddOpenApi();
builder.Services.AddDbContext<CrediteDbContext>(opcoes =>
    opcoes.UseNpgsql(builder.Configuration.GetConnectionString("BancoDados")));

var configuracaoKafka = builder.Configuration.GetSection("Kafka");
var servidoresKafka = configuracaoKafka["BootstrapServers"] ?? "localhost:9092";

builder.Services.AddSingleton<IProducer<string, string>>(_ =>
    new ProducerBuilder<string, string>(new ProducerConfig
    {
        BootstrapServers = servidoresKafka,
        ClientId = "credite-credit-api",
        MessageTimeoutMs = 15_000
    }).Build());

builder.Services.AddSingleton<IAdminClient>(_ =>
    new AdminClientBuilder(new AdminClientConfig
    {
        BootstrapServers = servidoresKafka,
        ClientId = "credite-credit-api-health"
    }).Build());

builder.Services.AddHostedService<PublicadorEventos>();

var app = builder.Build();

await using (var escopo = app.Services.CreateAsyncScope())
{
    var banco = escopo.ServiceProvider.GetRequiredService<CrediteDbContext>();
    await banco.Database.MigrateAsync();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapGet("/health/live", () => Results.Ok(new { status = "alive", servico = "credit" }));
app.MapGet("/health", VerificarProntidao);
app.MapClientesEndpoints();
app.MapSolicitacoesEndpoints();

await app.RunAsync();

static async Task<IResult> VerificarProntidao(
    CrediteDbContext banco,
    IAdminClient kafka,
    CancellationToken cancellationToken)
{
    var bancoDisponivel = await banco.Database.CanConnectAsync(cancellationToken);
    if (!bancoDisponivel)
    {
        return Results.Problem(
            title: "Banco de dados indisponível.",
            statusCode: StatusCodes.Status503ServiceUnavailable);
    }

    try
    {
        kafka.GetMetadata(TimeSpan.FromSeconds(3));
        return Results.Ok(new
        {
            status = "ready",
            servico = "credit",
            dependencias = new { postgresql = "healthy", kafka = "healthy" }
        });
    }
    catch (Exception excecao) when (excecao is KafkaException or TimeoutException)
    {
        return Results.Problem(
            title: "Kafka indisponível.",
            detail: "A API não está pronta para publicar eventos.",
            statusCode: StatusCodes.Status503ServiceUnavailable);
    }
}

public partial class Program { }
