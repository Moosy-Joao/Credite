using Credite.Credit.Api.Dominio;
using Xunit;

namespace Credite.Tests;

public sealed class SolicitacaoCreditoTests
{
    [Fact]
    public void DeveCriarSolicitacaoComStatusEmAnalise()
    {
        var clienteId = Guid.NewGuid();
        var solicitacao = new SolicitacaoCredito(clienteId, 2_500m);

        Assert.Equal(clienteId, solicitacao.ClienteId);
        Assert.Equal(2_500m, solicitacao.ValorSolicitado);
        Assert.Equal("EmAnalise", solicitacao.Status);
        Assert.NotEqual(Guid.Empty, solicitacao.Id);
    }

    [Theory]
    [InlineData(99.99)]
    [InlineData(10_000.01)]
    public void DeveRejeitarValorForaDoLimite(decimal valor)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new SolicitacaoCredito(Guid.NewGuid(), valor));
    }

    [Fact]
    public void DeveRejeitarClienteInvalido()
    {
        Assert.Throws<ArgumentException>(() => new SolicitacaoCredito(Guid.Empty, 500m));
    }

    [Fact]
    public void DeveRejeitarMaisDeDuasCasasDecimais()
    {
        Assert.Throws<ArgumentException>(() => new SolicitacaoCredito(Guid.NewGuid(), 100.123m));
    }
}
