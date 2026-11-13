using Confluent.Kafka;
using Credite.Notification.Worker.Dados;
using Credite.Notification.Worker.Servicos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddDbContext<NotificacoesDbContext>(opcoes =>
    opcoes.UseNpgsql(builder.Configuration.GetConnectionString("BancoDados")));

var configuracaoKafka = builder.Configuration.GetSection("Kafka");
builder.Services.AddSingleton<IProducer<string, string>>(_ =>
    new ProducerBuilder<string, string>(new ProducerConfig
    {
        BootstrapServers = configuracaoKafka["BootstrapServers"] ?? "localhost:9092",
        ClientId = "credite-notification-worker"
    }).Build());

builder.Services.AddHostedService<ConsumidorEventos>();

var host = builder.Build();

await using (var escopo = host.Services.CreateAsyncScope())
{
    var banco = escopo.ServiceProvider.GetRequiredService<NotificacoesDbContext>();
    await banco.Database.MigrateAsync();
}

await host.RunAsync();
