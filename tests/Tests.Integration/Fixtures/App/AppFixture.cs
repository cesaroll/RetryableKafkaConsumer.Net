using Aspire.Hosting;

namespace Tests.Integration.Tests.Fixtures.App;

public class AppFixture : IAsyncLifetime
{
    public IDistributedApplicationTestingBuilder? AppHost { get; private set; }
    public DistributedApplication? App { get; private set; }
    public ResourceNotificationService? ResourceNotificationService { get; private set; }
    public HttpClient? HttpClient { get; private set; }
    
    public async Task InitializeAsync()
    {
        AppHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.AppHost>();
        AppHost.Services.ConfigureHttpClientDefaults(clientBuilder =>
        {
            clientBuilder.AddStandardResilienceHandler();
        });

        App = await AppHost.BuildAsync();
        ResourceNotificationService = App.Services.GetRequiredService<ResourceNotificationService>();
        await App.StartAsync();

        HttpClient = App.CreateHttpClient("aspireconsumer");
        await ResourceNotificationService
            .WaitForResourceAsync("aspireconsumer", KnownResourceStates.Running)
            .WaitAsync(TimeSpan.FromSeconds(30));
        
        await Task.Delay(TimeSpan.FromSeconds(5));
    }

    public Task DisposeAsync()
    {
        HttpClient?.Dispose();
        App?.Dispose();
        return Task.CompletedTask;
    }
}