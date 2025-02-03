using Aspire.Hosting;
using Tests.Integration.Tests.Fixtures.App;

namespace Tests.Integration.Tests;

[Collection("AppCollection")]
public class PingTest
{
    private readonly AppFixture _appFixture;

    public PingTest(AppFixture appFixture)
    {
        _appFixture = appFixture;
    }
    
    [Fact]
    public async Task GetWebResourceRootReturnsOkStatusCode()
    {
        // Act
        var response = await _appFixture.HttpClient.GetAsync("/ping");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}