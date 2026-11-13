using Credite.Notification.Worker.Dominio;
using Microsoft.EntityFrameworkCore;

namespace Credite.Notification.Worker.Dados;

public sealed class NotificacoesDbContext(DbContextOptions<NotificacoesDbContext> opcoes) : DbContext(opcoes)
{
    public DbSet<Notificacao> Notificacoes => Set<Notificacao>();

    protected override void OnModelCreating(ModelBuilder modelo)
    {
        modelo.HasDefaultSchema("notificacao");

        modelo.Entity<Notificacao>(entidade =>
        {
            entidade.ToTable("notificacoes");
            entidade.HasKey(notificacao => notificacao.Id).HasName("pk_notificacoes");
            entidade.Property(notificacao => notificacao.Id).HasColumnName("id").ValueGeneratedNever();
            entidade.Property(notificacao => notificacao.EventoId).HasColumnName("evento_id").IsRequired();
            entidade.Property(notificacao => notificacao.SolicitacaoId).HasColumnName("solicitacao_id").IsRequired();
            entidade.Property(notificacao => notificacao.ClienteId).HasColumnName("cliente_id").IsRequired();
            entidade.Property(notificacao => notificacao.Mensagem).HasColumnName("mensagem").HasMaxLength(500).IsRequired();
            entidade.Property(notificacao => notificacao.CriadoEm).HasColumnName("criado_em").IsRequired();
            entidade.HasIndex(notificacao => notificacao.EventoId).IsUnique().HasDatabaseName("ix_notificacoes_evento_id");
        });
    }
}
