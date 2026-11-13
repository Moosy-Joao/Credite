using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Credite.Credit.Api.Dados;

public sealed class CrediteDbContextFactory : IDesignTimeDbContextFactory<CrediteDbContext>
{
    public CrediteDbContext CreateDbContext(string[] args)
    {
        var conexao = Environment.GetEnvironmentVariable("ConnectionStrings__BancoDados")
            ?? "Host=localhost;Port=5432;Database=credito;Username=credite;Password=credite_dev_password";

        var opcoes = new DbContextOptionsBuilder<CrediteDbContext>()
            .UseNpgsql(conexao)
            .Options;

        return new CrediteDbContext(opcoes);
    }
}
