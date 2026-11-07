using Microsoft.AspNetCore.Identity;

namespace Credite.Identity.Infrastructure.Identity;

/// <summary>
/// Conta de acesso ao sistema (ADR-0002). Fica na Infrastructure para que o Domain
/// não dependa de Identity. Não é o cliente fictício: contas de acesso e clientes são separados.
/// </summary>
public sealed class ApplicationUser : IdentityUser<Guid>
{
}
