using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Credite.Notification.Worker.Dados;

public sealed class NotificacoesDbContextFactory : IDesignTimeDbContextFactory<NotificacoesDbContext>
{
    public NotificacoesDbContext CreateDbContext(string[] args)
    {
        var conexao = Environment.GetEnvironmentVariable("ConnectionStrings__BancoDados")
            ?? "Host=localhost;Port=5432;Database=notificacoes;Username=credite;Password=credite_dev_password";

        var opcoes = new DbContextOptionsBuilder<NotificacoesDbContext>()
            .UseNpgsql(conexao)
            .Options;

        return new NotificacoesDbContext(opcoes);
    }
}
