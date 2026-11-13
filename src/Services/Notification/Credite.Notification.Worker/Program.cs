using Credite.Notification.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddHostedService<ProcessadorNotificacoes>();

await builder.Build().RunAsync();
