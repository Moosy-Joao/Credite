using System.Xml.Linq;
using Xunit;

namespace Credite.Architecture.Tests;

/// <summary>
/// Protege a estrutura de microsserviços do Credite (ADR-0014). As regras leem os
/// arquivos .csproj, pois o compilador omite dos metadados as referências que o
/// código nunca usa.
/// </summary>
public sealed class RegrasDeArquiteturaTestes
{
    private static readonly string[] Servicos = ["Identity", "Customer", "Credit", "Notification"];

    private static readonly string[] ProjetosPrevistos =
    [
        "Credite.Web",
        "Credite.Identity.Domain",
        "Credite.Identity.Application",
        "Credite.Identity.Infrastructure",
        "Credite.Identity.Api",
        "Credite.Customer.Domain",
        "Credite.Customer.Application",
        "Credite.Customer.Infrastructure",
        "Credite.Customer.Api",
        "Credite.Credit.Domain",
        "Credite.Credit.Application",
        "Credite.Credit.Infrastructure",
        "Credite.Credit.Api",
        // O Notification não tem Domain: ele só será criado quando houver regras de negócio próprias.
        "Credite.Notification.Application",
        "Credite.Notification.Infrastructure",
        "Credite.Notification.Worker"
    ];

    // TRANSIÇÃO TÉCNICA (Etapa 2): o Identity ainda usa SQL Server. Esta lista de exceções
    // DEVE ser esvaziada na Etapa 3, quando o Identity migrar para PostgreSQL. A partir daí,
    // o teste PacotesDeSqlServerSoDevemExistirNaTransicaoDoIdentity passa a proibir o SQL Server.
    private static readonly string[] ProjetosComExcecaoDeSqlServer =
    [
        "Credite.Identity.Infrastructure",
        "Credite.Identity.IntegrationTests"
    ];

    private static readonly string[] PacotesDeSqlServer =
    [
        "Microsoft.EntityFrameworkCore.SqlServer",
        "Testcontainers.MsSql"
    ];

    private sealed record Projeto(string Nome, string[] Referencias, string[] Pacotes);

    private static readonly IReadOnlyList<Projeto> Projetos = CarregarProjetos();

    // Sobe a partir da pasta de saída do teste até encontrar o Credite.slnx.
    private static string ObterRaizDaSolucao()
    {
        var diretorio = new DirectoryInfo(AppContext.BaseDirectory);

        while (diretorio is not null
               && !File.Exists(Path.Combine(diretorio.FullName, "Credite.slnx")))
        {
            diretorio = diretorio.Parent;
        }

        return diretorio?.FullName
            ?? throw new InvalidOperationException("Credite.slnx não encontrado.");
    }

    private static IReadOnlyList<Projeto> CarregarProjetos()
    {
        var raiz = ObterRaizDaSolucao();

        return Directory.EnumerateFiles(raiz, "*.csproj", SearchOption.AllDirectories)
            .Select(CarregarProjeto)
            .OrderBy(projeto => projeto.Nome, StringComparer.Ordinal)
            .ToList();
    }

    private static Projeto CarregarProjeto(string caminho)
    {
        var documento = XDocument.Load(caminho);

        var referencias = documento.Descendants("ProjectReference")
            // O .csproj gerado pelo Visual Studio pode usar '\'; no CI (Linux) só '/' separa pastas.
            .Select(x => ((string)x.Attribute("Include")!).Replace('\\', '/'))
            .Select(Path.GetFileNameWithoutExtension)
            .OfType<string>()
            .ToArray();

        var pacotes = documento.Descendants("PackageReference")
            .Select(x => (string?)x.Attribute("Include"))
            .OfType<string>()
            .ToArray();

        return new Projeto(Path.GetFileNameWithoutExtension(caminho), referencias, pacotes);
    }

    private static Projeto? Buscar(string nome)
        => Projetos.SingleOrDefault(projeto => projeto.Nome == nome);

    private static string Nome(string servico, string camada)
        => $"Credite.{servico}.{camada}";

    // O Notification é um Worker Service; os demais serviços têm uma Api.
    private static string NomeDoHost(string servico)
        => servico == "Notification" ? "Worker" : "Api";

    // Falha se o projeto referenciar algo fora da lista de permitidos. Projeto ausente não
    // falha aqui: a ausência é reportada por TodosOsProjetosPrevistosDevemExistir.
    private static void AfirmarSomentePermitidas(string projeto, params string[] permitidas)
    {
        var alvo = Buscar(projeto);

        if (alvo is null)
        {
            return;
        }

        var proibidas = alvo.Referencias.Except(permitidas).ToArray();

        Assert.Empty(proibidas);
    }

    [Fact]
    public void TodosOsProjetosPrevistosDevemExistir()
    {
        var existentes = Projetos.Select(projeto => projeto.Nome).ToHashSet();

        var ausentes = ProjetosPrevistos.Where(nome => !existentes.Contains(nome)).ToArray();

        Assert.Empty(ausentes);
    }

    [Theory]
    [InlineData("Identity")]
    [InlineData("Customer")]
    [InlineData("Credit")]
    [InlineData("Notification")]
    public void DominioNaoDeveReferenciarOutrosProjetos(string servico)
        => AfirmarSomentePermitidas(Nome(servico, "Domain"));

    [Theory]
    [InlineData("Identity")]
    [InlineData("Customer")]
    [InlineData("Credit")]
    [InlineData("Notification")]
    public void AplicacaoDeveReferenciarSomenteODominio(string servico)
        => AfirmarSomentePermitidas(Nome(servico, "Application"), Nome(servico, "Domain"));

    [Theory]
    [InlineData("Identity")]
    [InlineData("Customer")]
    [InlineData("Credit")]
    [InlineData("Notification")]
    public void InfraestruturaDeveReferenciarSomenteAplicacaoEDominio(string servico)
        => AfirmarSomentePermitidas(
            Nome(servico, "Infrastructure"),
            Nome(servico, "Application"),
            Nome(servico, "Domain"));

    // O host é a raiz de composição: depende da Application e da Infrastructure e não
    // referencia o Domain diretamente.
    [Theory]
    [InlineData("Identity")]
    [InlineData("Customer")]
    [InlineData("Credit")]
    [InlineData("Notification")]
    public void HostDeveReferenciarSomenteAplicacaoEInfraestrutura(string servico)
        => AfirmarSomentePermitidas(
            Nome(servico, NomeDoHost(servico)),
            Nome(servico, "Application"),
            Nome(servico, "Infrastructure"));

    // Isolamento: nenhum projeto de um serviço (inclusive os de teste) referencia projetos
    // de outro serviço. Toda comunicação entre serviços é por contrato, nunca por referência.
    [Theory]
    [InlineData("Identity")]
    [InlineData("Customer")]
    [InlineData("Credit")]
    [InlineData("Notification")]
    public void ProjetosDeUmServicoNaoDevemReferenciarOutrosServicos(string servico)
    {
        var prefixoProprio = $"Credite.{servico}.";
        var prefixosAlheios = Servicos
            .Where(outro => outro != servico)
            .Select(outro => $"Credite.{outro}.")
            .ToArray();

        var violacoes = Projetos
            .Where(projeto => projeto.Nome.StartsWith(prefixoProprio, StringComparison.Ordinal))
            .SelectMany(projeto => projeto.Referencias.Select(referencia => (Projeto: projeto.Nome, Referencia: referencia)))
            .Where(x => prefixosAlheios.Any(prefixo => x.Referencia.StartsWith(prefixo, StringComparison.Ordinal)))
            .Select(x => $"{x.Projeto} -> {x.Referencia}")
            .ToArray();

        Assert.Empty(violacoes);
    }

    // A Web é cliente da plataforma: não referencia projetos de serviços (ADR-0014).
    // A forma de integração continua pendente da ADR-0015.
    [Fact]
    public void WebNaoDeveReferenciarProjetosDeServicos()
    {
        var web = Buscar("Credite.Web");

        Assert.NotNull(web);

        var violacoes = web.Referencias
            .Where(referencia => Servicos.Any(servico =>
                referencia.StartsWith($"Credite.{servico}.", StringComparison.Ordinal)))
            .ToArray();

        Assert.Empty(violacoes);
    }

    // Persistência poliglota (ADR-0014): MongoDB só no Notification e Npgsql nunca nele.
    [Fact]
    public void PacotesDeBancoDevemRespeitarAPropriedadeDosDados()
    {
        var violacoes = Projetos
            .SelectMany(projeto => projeto.Pacotes.Select(pacote => (Projeto: projeto.Nome, Pacote: pacote)))
            .Where(x => ViolaPropriedadeDosDados(x.Projeto, x.Pacote))
            .Select(x => $"{x.Projeto}: {x.Pacote}")
            .ToArray();

        Assert.Empty(violacoes);
    }

    private static bool ViolaPropriedadeDosDados(string projeto, string pacote)
    {
        var ehNotification = projeto.StartsWith("Credite.Notification.", StringComparison.Ordinal);
        var ehMongo = pacote.StartsWith("MongoDB.", StringComparison.Ordinal);
        var ehPostgres = pacote.StartsWith("Npgsql", StringComparison.Ordinal);

        return (ehMongo && !ehNotification) || (ehPostgres && ehNotification);
    }

    // TRANSIÇÃO TÉCNICA (Etapa 2): SQL Server só é tolerado nos projetos do Identity listados em
    // ProjetosComExcecaoDeSqlServer. A Etapa 3 deve esvaziar essa lista.
    [Fact]
    public void PacotesDeSqlServerSoDevemExistirNaTransicaoDoIdentity()
    {
        var violacoes = Projetos
            .Where(projeto => !ProjetosComExcecaoDeSqlServer.Contains(projeto.Nome))
            .SelectMany(projeto => projeto.Pacotes
                .Where(pacote => PacotesDeSqlServer.Contains(pacote))
                .Select(pacote => $"{projeto.Nome}: {pacote}"))
            .ToArray();

        Assert.Empty(violacoes);
    }
}
