using Credite.Credit.Api.Dominio;
using Microsoft.EntityFrameworkCore;

namespace Credite.Credit.Api.Dados;

public sealed class CrediteDbContext(DbContextOptions<CrediteDbContext> opcoes) : DbContext(opcoes)
{
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<SolicitacaoCredito> Solicitacoes => Set<SolicitacaoCredito>();
    public DbSet<EventoPendente> EventosPendentes => Set<EventoPendente>();

    protected override void OnModelCreating(ModelBuilder modelo)
    {
        modelo.HasDefaultSchema("credito");

        modelo.Entity<Cliente>(entidade =>
        {
            entidade.ToTable("clientes");
            entidade.HasKey(cliente => cliente.Id).HasName("pk_clientes");
            entidade.Property(cliente => cliente.Id).HasColumnName("id").ValueGeneratedNever();
            entidade.Property(cliente => cliente.Nome).HasColumnName("nome").HasMaxLength(100).IsRequired();
            entidade.Property(cliente => cliente.Email).HasColumnName("email").HasMaxLength(254).IsRequired();
            entidade.Property(cliente => cliente.CriadoEm).HasColumnName("criado_em").IsRequired();
            entidade.HasIndex(cliente => cliente.Email).IsUnique().HasDatabaseName("ix_clientes_email");
        });

        modelo.Entity<SolicitacaoCredito>(entidade =>
        {
            entidade.ToTable("solicitacoes_credito");
            entidade.HasKey(solicitacao => solicitacao.Id).HasName("pk_solicitacoes_credito");
            entidade.Property(solicitacao => solicitacao.Id).HasColumnName("id").ValueGeneratedNever();
            entidade.Property(solicitacao => solicitacao.ClienteId).HasColumnName("cliente_id").IsRequired();
            entidade.Property(solicitacao => solicitacao.ValorSolicitado).HasColumnName("valor_solicitado").HasPrecision(10, 2);
            entidade.Property(solicitacao => solicitacao.Status).HasColumnName("status").HasMaxLength(30).IsRequired();
            entidade.Property(solicitacao => solicitacao.CriadoEm).HasColumnName("criado_em").IsRequired();
            entidade.HasOne<Cliente>().WithMany().HasForeignKey(solicitacao => solicitacao.ClienteId).OnDelete(DeleteBehavior.Restrict);
            entidade.HasIndex(solicitacao => solicitacao.ClienteId).HasDatabaseName("ix_solicitacoes_credito_cliente_id");
        });

        modelo.Entity<EventoPendente>(entidade =>
        {
            entidade.ToTable("eventos_pendentes");
            entidade.HasKey(evento => evento.Id).HasName("pk_eventos_pendentes");
            entidade.Property(evento => evento.Id).HasColumnName("id").ValueGeneratedNever();
            entidade.Property(evento => evento.Tipo).HasColumnName("tipo").HasMaxLength(150).IsRequired();
            entidade.Property(evento => evento.Conteudo).HasColumnName("conteudo").IsRequired();
            entidade.Property(evento => evento.CriadoEm).HasColumnName("criado_em").IsRequired();
            entidade.Property(evento => evento.PublicadoEm).HasColumnName("publicado_em");
            entidade.Property(evento => evento.Tentativas).HasColumnName("tentativas").IsRequired();
            entidade.Property(evento => evento.UltimoErro).HasColumnName("ultimo_erro").HasMaxLength(500);
            entidade.HasIndex(evento => new { evento.PublicadoEm, evento.CriadoEm }).HasDatabaseName("ix_eventos_pendentes_publicacao");
        });
    }
}
