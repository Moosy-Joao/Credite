using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Credite.Notification.Worker.Dados;

[DbContext(typeof(NotificacoesDbContext))]
[Migration("202611130001_MigracaoInicial")]
public sealed class MigracaoInicial : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(name: "notificacao");

        migrationBuilder.CreateTable(
            name: "notificacoes",
            schema: "notificacao",
            columns: tabela => new
            {
                id = tabela.Column<Guid>(type: "uuid", nullable: false),
                evento_id = tabela.Column<Guid>(type: "uuid", nullable: false),
                solicitacao_id = tabela.Column<Guid>(type: "uuid", nullable: false),
                cliente_id = tabela.Column<Guid>(type: "uuid", nullable: false),
                mensagem = tabela.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                criado_em = tabela.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: tabela => tabela.PrimaryKey("pk_notificacoes", x => x.id));

        migrationBuilder.CreateIndex(
            name: "ix_notificacoes_evento_id",
            schema: "notificacao",
            table: "notificacoes",
            column: "evento_id",
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "notificacoes", schema: "notificacao");
    }
}

[DbContext(typeof(NotificacoesDbContext))]
public sealed class NotificacoesDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelo)
    {
        modelo.HasAnnotation("ProductVersion", "10.0.15");
        modelo.HasDefaultSchema("notificacao");

        modelo.Entity("Credite.Notification.Worker.Dominio.Notificacao", entidade =>
        {
            entidade.Property<Guid>("Id").HasColumnType("uuid").HasColumnName("id");
            entidade.Property<Guid>("ClienteId").HasColumnType("uuid").HasColumnName("cliente_id");
            entidade.Property<DateTimeOffset>("CriadoEm").HasColumnType("timestamp with time zone").HasColumnName("criado_em");
            entidade.Property<Guid>("EventoId").HasColumnType("uuid").HasColumnName("evento_id");
            entidade.Property<string>("Mensagem").HasMaxLength(500).HasColumnType("character varying(500)").HasColumnName("mensagem");
            entidade.Property<Guid>("SolicitacaoId").HasColumnType("uuid").HasColumnName("solicitacao_id");
            entidade.HasKey("Id").HasName("pk_notificacoes");
            entidade.HasIndex("EventoId").IsUnique().HasDatabaseName("ix_notificacoes_evento_id");
            entidade.ToTable("notificacoes", "notificacao");
        });
    }
}
