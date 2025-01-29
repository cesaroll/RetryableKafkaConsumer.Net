using Microsoft.Extensions.Hosting;
using RetryableConsumer.Domain.Configs;
using RetryableConsumer.Processor.Factories;
using RetryableConsumer.Tasks;

namespace RetryableConsumer.Services;

public class ConsumerHostedService : IHostedService
{
    private readonly IProcessorFactory _processorFactory;
    private readonly RegistrationConfig _config;

    public ConsumerHostedService(
        IProcessorFactory processorFactory,
        RegistrationConfig config)
    {
        _processorFactory = processorFactory;
        _config = config;
    }

    public async Task StartAsync(CancellationToken ct)
    {
        var consumers = _processorFactory
            .CreateProcessors(_config)
            .Select(processor => new ConsumerTask(processor));
        
        var tasks = consumers.Select(consumer => consumer.Run(ct));
        
        await Task.Run(() => Task.WhenAll(tasks), ct);
    }

    public Task StopAsync(CancellationToken ct)
        => Task.CompletedTask;
}