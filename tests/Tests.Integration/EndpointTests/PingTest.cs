using Tests.Integration.Tests.Fixtures.App;

namespace Tests.Integration.EndpointTests;

[Collection("AppCollection")]
public class PingTest
{
    private readonly HttpClient _httpClient;

    public PingTest(AppFixture appFixture)
    {
        _httpClient = appFixture.HttpClient!;
    }
    
    [Fact(Skip = "Test is ignored")]
    public async Task GetWebResourceRootReturnsOkStatusCode()
    {
        // Act
        var response = await _httpClient.GetAsync("/ping");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}