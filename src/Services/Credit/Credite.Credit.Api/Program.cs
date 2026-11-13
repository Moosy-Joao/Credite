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
builder.Services.AddSingleton<IProducer<string, string>>(_ =>
    new ProducerBuilder<string, string>(new ProducerConfig
    {
        BootstrapServers = configuracaoKafka["BootstrapServers"] ?? "localhost:9092",
        ClientId = "credite-credit-api"
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

app.MapGet("/health", VerificarSaude);
app.MapClientesEndpoints();
app.MapSolicitacoesEndpoints();

await app.RunAsync();

static async Task<IResult> VerificarSaude(
    CrediteDbContext banco,
    CancellationToken cancellationToken)
{
    var bancoDisponivel = await banco.Database.CanConnectAsync(cancellationToken);

    return bancoDisponivel
        ? Results.Ok(new { status = "healthy", servico = "credit" })
        : Results.Problem(
            title: "Banco de dados indisponível.",
            statusCode: StatusCodes.Status503ServiceUnavailable);
}

public partial class Program { }
