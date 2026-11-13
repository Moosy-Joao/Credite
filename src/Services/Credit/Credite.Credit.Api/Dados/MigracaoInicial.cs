using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Credite.Credit.Api.Dados;

[DbContext(typeof(CrediteDbContext))]
[Migration("202611130001_MigracaoInicial")]
public sealed class MigracaoInicial : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(name: "credito");

        migrationBuilder.CreateTable(
            name: "clientes",
            schema: "credito",
            columns: tabela => new
            {
                id = tabela.Column<Guid>(type: "uuid", nullable: false),
                nome = tabela.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                email = tabela.Column<string>(type: "character varying(254)", maxLength: 254, nullable: false),
                criado_em = tabela.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: tabela => tabela.PrimaryKey("pk_clientes", x => x.id));

        migrationBuilder.CreateTable(
            name: "eventos_pendentes",
            schema: "credito",
            columns: tabela => new
            {
                id = tabela.Column<Guid>(type: "uuid", nullable: false),
                tipo = tabela.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                conteudo = tabela.Column<string>(type: "text", nullable: false),
                criado_em = tabela.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                publicado_em = tabela.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                tentativas = tabela.Column<int>(type: "integer", nullable: false),
                ultimo_erro = tabela.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
            },
            constraints: tabela => tabela.PrimaryKey("pk_eventos_pendentes", x => x.id));

        migrationBuilder.CreateTable(
            name: "solicitacoes_credito",
            schema: "credito",
            columns: tabela => new
            {
                id = tabela.Column<Guid>(type: "uuid", nullable: false),
                cliente_id = tabela.Column<Guid>(type: "uuid", nullable: false),
                valor_solicitado = tabela.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                status = tabela.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                criado_em = tabela.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: tabela =>
            {
                tabela.PrimaryKey("pk_solicitacoes_credito", x => x.id);
                tabela.ForeignKey(
                    name: "fk_solicitacoes_credito_clientes_cliente_id",
                    column: x => x.cliente_id,
                    principalTable: "clientes",
                    principalColumn: "id",
                    principalSchema: "credito",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex("ix_clientes_email", "clientes", "email", "credito", unique: true);
        migrationBuilder.CreateIndex("ix_eventos_pendentes_publicacao", "eventos_pendentes",
            new[] { "publicado_em", "criado_em" }, "credito");
        migrationBuilder.CreateIndex("ix_solicitacoes_credito_cliente_id", "solicitacoes_credito",
            "cliente_id", "credito");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable("eventos_pendentes", "credito");
        migrationBuilder.DropTable("solicitacoes_credito", "credito");
        migrationBuilder.DropTable("clientes", "credito");
    }

    protected override void BuildTargetModel(ModelBuilder modelo) =>
        ModeloCredite.Configurar(modelo);
}

[DbContext(typeof(CrediteDbContext))]
public sealed class CrediteDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelo) =>
        ModeloCredite.Configurar(modelo);
}

internal static class ModeloCredite
{
    public static void Configurar(ModelBuilder modelo)
    {
        modelo.HasDefaultSchema("credito");
        modelo.HasAnnotation("ProductVersion", "10.0.15");
        modelo.HasAnnotation("Relational:MaxIdentifierLength", 63);
        NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelo);

        modelo.Entity("Credite.Credit.Api.Dominio.Cliente", entidade =>
        {
            entidade.Property<Guid>("Id").ValueGeneratedNever().HasColumnType("uuid").HasColumnName("id");
            entidade.Property<string>("Email").IsRequired().HasMaxLength(254).HasColumnType("character varying(254)").HasColumnName("email");
            entidade.Property<string>("Nome").IsRequired().HasMaxLength(100).HasColumnType("character varying(100)").HasColumnName("nome");
            entidade.Property<DateTimeOffset>("CriadoEm").IsRequired().HasColumnType("timestamp with time zone").HasColumnName("criado_em");
            entidade.HasKey("Id").HasName("pk_clientes");
            entidade.HasIndex("Email").IsUnique().HasDatabaseName("ix_clientes_email");
            entidade.ToTable("clientes", "credito");
        });

        modelo.Entity("Credite.Credit.Api.Dominio.EventoPendente", entidade =>
        {
            entidade.Property<Guid>("Id").ValueGeneratedNever().HasColumnType("uuid").HasColumnName("id");
            entidade.Property<string>("Conteudo").IsRequired().HasColumnType("text").HasColumnName("conteudo");
            entidade.Property<DateTimeOffset>("CriadoEm").IsRequired().HasColumnType("timestamp with time zone").HasColumnName("criado_em");
            entidade.Property<DateTimeOffset?>("PublicadoEm").HasColumnType("timestamp with time zone").HasColumnName("publicado_em");
            entidade.Property<int>("Tentativas").IsRequired().HasColumnType("integer").HasColumnName("tentativas");
            entidade.Property<string>("Tipo").IsRequired().HasMaxLength(150).HasColumnType("character varying(150)").HasColumnName("tipo");
            entidade.Property<string>("UltimoErro").HasMaxLength(500).HasColumnType("character varying(500)").HasColumnName("ultimo_erro");
            entidade.HasKey("Id").HasName("pk_eventos_pendentes");
            entidade.HasIndex("PublicadoEm", "CriadoEm").HasDatabaseName("ix_eventos_pendentes_publicacao");
            entidade.ToTable("eventos_pendentes", "credito");
        });

        modelo.Entity("Credite.Credit.Api.Dominio.SolicitacaoCredito", entidade =>
        {
            entidade.Property<Guid>("Id").ValueGeneratedNever().HasColumnType("uuid").HasColumnName("id");
            entidade.Property<Guid>("ClienteId").IsRequired().HasColumnType("uuid").HasColumnName("cliente_id");
            entidade.Property<DateTimeOffset>("CriadoEm").IsRequired().HasColumnType("timestamp with time zone").HasColumnName("criado_em");
            entidade.Property<string>("Status").IsRequired().HasMaxLength(30).HasColumnType("character varying(30)").HasColumnName("status");
            entidade.Property<decimal>("ValorSolicitado").IsRequired().HasPrecision(10, 2).HasColumnType("numeric(10,2)").HasColumnName("valor_solicitado");
            entidade.HasKey("Id").HasName("pk_solicitacoes_credito");
            entidade.HasIndex("ClienteId").HasDatabaseName("ix_solicitacoes_credito_cliente_id");
            entidade.ToTable("solicitacoes_credito", "credito");
            entidade.HasOne("Credite.Credit.Api.Dominio.Cliente", null)
                .WithMany().HasForeignKey("ClienteId").OnDelete(DeleteBehavior.Restrict).IsRequired();
        });
    }
}
