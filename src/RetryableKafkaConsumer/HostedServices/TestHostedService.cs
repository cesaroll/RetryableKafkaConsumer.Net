using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace RetryableKafkaConsumer.HostedServices;

public class TestHostedService : BackgroundService //IHostedService //IHostedLifecycleService
{
    private readonly ILogger<TestHostedService> _logger;

    public TestHostedService(ILogger<TestHostedService> logger)
    {
        _logger = logger;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        throw new NotImplementedException();
    }
    
    // public async Task StartAsync(CancellationToken cancellationToken)
    // {
    //     _logger.LogInformation("TestHostedService is starting.");
    // }
    //
    // public async Task StopAsync(CancellationToken cancellationToken)
    // {
    //     _logger.LogInformation("TestHostedService is stopping.");
    // }

    // public Task StartingAsync(CancellationToken cancellationToken)
    // {
    //     throw new NotImplementedException();
    // }
    //
    // public Task StartedAsync(CancellationToken cancellationToken)
    // {
    //     throw new NotImplementedException();
    // }
    //
    // public Task StoppingAsync(CancellationToken cancellationToken)
    // {
    //     throw new NotImplementedException();
    // }
    //
    // public Task StoppedAsync(CancellationToken cancellationToken)
    // {
    //     throw new NotImplementedException();
    // }
    
}