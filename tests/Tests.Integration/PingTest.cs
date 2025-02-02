using Aspire.Hosting;

namespace Tests.Integration.Tests;

public class PingTest : IAsyncLifetime
{
    private IDistributedApplicationTestingBuilder _appHost;
    private DistributedApplication _app;
    private ResourceNotificationService _resourceNotificationService;
    private HttpClient _httpClient;
    
    public async Task InitializeAsync()
    {
        _appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.AppHost>();
        _appHost.Services.ConfigureHttpClientDefaults(clientBuilder =>
        {
            clientBuilder.AddStandardResilienceHandler();
        });
        
        _app = await _appHost.BuildAsync();
        _resourceNotificationService = _app.Services.GetRequiredService<ResourceNotificationService>();
        await _app.StartAsync();
        
        _httpClient = _app.CreateHttpClient("aspireconsumer");
        await _resourceNotificationService
            .WaitForResourceAsync("aspireconsumer", KnownResourceStates.Running)
            .WaitAsync(TimeSpan.FromSeconds(30));
    }

    public Task DisposeAsync()
    {
        _httpClient?.Dispose();
        _app?.Dispose();
        return Task.CompletedTask;
    }
    
    [Fact]
    public async Task GetWebResourceRootReturnsOkStatusCode()
    {
        // Act
        var response = await _httpClient.GetAsync("/ping");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}