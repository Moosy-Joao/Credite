using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Credite.Notification.Worker;

public sealed class ProcessadorNotificacoes(
    ILogger<ProcessadorNotificacoes> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Notification Worker iniciado.");

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
}
