using System.Xml.Linq;
using Xunit;

namespace Credite.UnitTests;

/// <summary>
/// Protege as dependências permitidas entre projetos, conforme o ADR-0005.
/// Os testes leem os arquivos .csproj, pois o compilador omite dos metadados
/// as referências a assemblies que o código nunca usa.
/// </summary>
public sealed class ArchitectureTests
{
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

    // Devolve os nomes dos projetos referenciados por um projeto da solução.
    private static string[] ObterReferencias(string pasta, string projeto)
    {
        var caminho = Path.Combine(ObterRaizDaSolucao(), pasta, projeto, $"{projeto}.csproj");

        return XDocument.Load(caminho)
            .Descendants("ProjectReference")
            // O .csproj gerado pelo Visual Studio pode usar '\'; no CI (Linux) só '/' separa pastas.
            .Select(x => ((string)x.Attribute("Include")!).Replace('\\', '/'))
            .Select(Path.GetFileNameWithoutExtension)
            .OfType<string>()
            .ToArray();
    }

    // Falha se o projeto referenciar algo fora da lista de permitidos.
    // A mensagem de falha lista exatamente as referências proibidas.
    private static void AssertSomentePermitidas(string projeto, params string[] permitidas)
    {
        var proibidas = ObterReferencias("src", projeto).Except(permitidas).ToArray();

        Assert.Empty(proibidas);
    }

    [Fact]
    public void Domain_ShouldNotReferenceOtherProjects()
        => AssertSomentePermitidas("Credite.Domain");

    [Fact]
    public void Application_ShouldReferenceOnlyDomain()
        => AssertSomentePermitidas("Credite.Application", "Credite.Domain");

    [Fact]
    public void Infrastructure_ShouldReferenceOnlyApplicationAndDomain()
        => AssertSomentePermitidas("Credite.Infrastructure", "Credite.Application", "Credite.Domain");

    // Credite.Contracts ainda não existe, mas o ADR-0005 o permite na Api.
    [Fact]
    public void Api_ShouldReferenceOnlyApplicationInfrastructureAndContracts()
        => AssertSomentePermitidas("Credite.Api", "Credite.Application", "Credite.Infrastructure", "Credite.Contracts");

    // A Web consome a API só por HTTP: o ADR-0005 permite apenas Contracts.
    [Fact]
    public void Web_ShouldReferenceOnlyContracts()
        => AssertSomentePermitidas("Credite.Web", "Credite.Contracts");
}
