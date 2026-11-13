using Credite.Credit.Api.Dominio;

namespace Credite.Tests;

public sealed class ClienteTests
{
    [Fact]
    public void DeveNormalizarNomeEEmail()
    {
        var cliente = new Cliente("  Ana Silva  ", " ANA@EXEMPLO.COM ");
        Assert.Equal("Ana Silva", cliente.Nome);
        Assert.Equal("ana@exemplo.com", cliente.Email);
        Assert.NotEqual(Guid.Empty, cliente.Id);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("A")]
    public void DeveRejeitarNomeInvalido(string nome)
    {
        Assert.Throws<ArgumentException>(() => new Cliente(nome, "ana@exemplo.com"));
    }

    [Theory]
    [InlineData("email-invalido")]
    [InlineData("ana@")]
    [InlineData("@exemplo.com")]
    public void DeveRejeitarEmailInvalido(string email)
    {
        Assert.Throws<ArgumentException>(() => new Cliente("Ana Silva", email));
    }
}
