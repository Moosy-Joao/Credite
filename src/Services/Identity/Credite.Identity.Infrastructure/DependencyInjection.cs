using Credite.Identity.Infrastructure.Identity;
using Credite.Identity.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Credite.Identity.Infrastructure;

public static class DependencyInjection
{
    /// <summary>
    /// Registra persistência e Identity. A connection string vem do host (User Secrets
    /// no desenvolvimento), para que a Infrastructure não conheça chaves de configuração.
    /// </summary>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        services.AddDbContext<CrediteDbContext>(options =>
            options.UseSqlServer(connectionString));

        // Só os serviços de Identity necessários para contas, senhas e funções.
        // Cookies e login (SignInManager) são configurados no host da API, na etapa 6.
        services.AddIdentityCore<ApplicationUser>()
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<CrediteDbContext>();

        return services;
    }
}
