using Credite.Identity.Infrastructure.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.MsSql;
using Xunit;

namespace Credite.Identity.IntegrationTests.Fixtures;

/// <summary>
/// Sobe um SQL Server descartável e a API apontando para ele.
/// É compartilhado por todos os testes da coleção, para iniciar o contêiner uma única vez.
/// </summary>
public sealed class IntegrationTestFixture : IAsyncLifetime
{
    private const string VariavelDeConexao = "ConnectionStrings__CrediteDb";

    private readonly MsSqlContainer _sqlServer =
        new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest").Build();

    public CrediteApiFactory Factory { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await _sqlServer.StartAsync();

        // Banco próprio dos testes; o padrão do contêiner seria o master.
        var conexao = new SqlConnectionStringBuilder(_sqlServer.GetConnectionString())
        {
            InitialCatalog = "CrediteTests"
        }.ConnectionString;

        // Variáveis de ambiente têm precedência sobre appsettings e User Secrets.
        Environment.SetEnvironmentVariable(VariavelDeConexao, conexao);
        Factory = new CrediteApiFactory();

        // Aplica as migrations explicitamente, como em qualquer outro ambiente.
        await using var escopo = Factory.Services.CreateAsyncScope();
        var db = escopo.ServiceProvider.GetRequiredService<CrediteDbContext>();
        await db.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        if (Factory is not null)
        {
            await Factory.DisposeAsync();
        }

        Environment.SetEnvironmentVariable(VariavelDeConexao, null);
        await _sqlServer.DisposeAsync();
    }
}
