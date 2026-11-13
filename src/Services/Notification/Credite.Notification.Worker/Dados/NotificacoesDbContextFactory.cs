using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Credite.Notification.Worker.Dados;

public sealed class NotificacoesDbContextFactory : IDesignTimeDbContextFactory<NotificacoesDbContext>
{
    public NotificacoesDbContext CreateDbContext(string[] args)
    {
        var opcoes = new DbContextOptionsBuilder<NotificacoesDbContext>()
            .UseNpgsql("Host=localhost;Port=5432;Database=notificacoes;Username=credite;Password=credite_dev_password")
            .Options;

        return new NotificacoesDbContext(opcoes);
    }
}
