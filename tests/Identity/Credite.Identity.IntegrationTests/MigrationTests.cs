using Credite.Identity.Infrastructure.Persistence;
using Credite.Identity.IntegrationTests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Credite.Identity.IntegrationTests;

[Collection(IntegrationTestCollection.Name)]
public sealed class MigrationTests(IntegrationTestFixture fixture)
{
    [Fact]
    public async Task Migrations_ShouldCreateIdentitySchemaInEmptyDatabase()
    {
        await using var escopo = fixture.Factory.Services.CreateAsyncScope();
        var db = escopo.ServiceProvider.GetRequiredService<CrediteDbContext>();

        var pendentes = await db.Database.GetPendingMigrationsAsync();
        var aplicadas = await db.Database.GetAppliedMigrationsAsync();

        Assert.Empty(pendentes);
        Assert.Contains(aplicadas, nome => nome.EndsWith("_InitialIdentity"));

        // Consultas simples comprovam que as tabelas existem e estão mapeadas.
        Assert.Equal(0, await db.Users.CountAsync());
        Assert.Equal(0, await db.Roles.CountAsync());
    }
}
