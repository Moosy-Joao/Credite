using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Credite.Credit.Api.Dados;

public sealed class CrediteDbContextFactory : IDesignTimeDbContextFactory<CrediteDbContext>
{
    public CrediteDbContext CreateDbContext(string[] args)
    {
        var opcoes = new DbContextOptionsBuilder<CrediteDbContext>()
            .UseNpgsql("Host=localhost;Port=5432;Database=credito;Username=credite;Password=credite_dev_password")
            .Options;

        return new CrediteDbContext(opcoes);
    }
}
