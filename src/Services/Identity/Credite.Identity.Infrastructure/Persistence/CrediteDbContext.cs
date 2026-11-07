using Credite.Identity.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Credite.Identity.Infrastructure.Persistence;

public sealed class CrediteDbContext(DbContextOptions<CrediteDbContext> options)
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>(options)
{
    protected override void OnModelCreating(ModelBuilder builder)
    {
        // Mantém o mapeamento padrão das tabelas de Identity.
        base.OnModelCreating(builder);
    }
}
