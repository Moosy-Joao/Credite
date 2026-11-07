using System.Net;
using Credite.Identity.IntegrationTests.Fixtures;
using Xunit;

namespace Credite.Identity.IntegrationTests.Api;

[Collection(IntegrationTestCollection.Name)]
public sealed class HealthTests(IntegrationTestFixture fixture)
{
    [Fact]
    public async Task Health_ShouldReturnOk_WhenDatabaseIsPrepared()
    {
        using var cliente = fixture.Factory.CreateClient();

        var resposta = await cliente.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
    }
}
