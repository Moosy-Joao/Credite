using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Credite.Identity.IntegrationTests.Fixtures;

/// <summary>Inicia a API real em memória para os testes.</summary>
public sealed class CrediteApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // "Testing" evita carregar User Secrets (que só ocorre em Development),
        // garantindo que os testes nunca usem o banco de desenvolvimento (ADR-0003).
        builder.UseEnvironment("Testing");
    }
}
