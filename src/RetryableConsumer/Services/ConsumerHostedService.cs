using Microsoft.Extensions.Hosting;
using RetryableConsumer.Domain.Configs;
using RetryableConsumer.Processor.Factories;
using RetryableConsumer.Tasks;

namespace RetryableConsumer.Services;

public class ConsumerHostedService : IHostedService
{
    private readonly IProcessorFactory _processorFactory;
    private readonly RegistrationConfig _config;
    private readonly TaskCompletionSource<bool> _kafkaInitcompletionSource;

    public ConsumerHostedService(
        IProcessorFactory processorFactory,
        RegistrationConfig config, 
        TaskCompletionSource<bool> kafkaInitcompletionSource)
    {
        _processorFactory = processorFactory;
        _config = config;
        _kafkaInitcompletionSource = kafkaInitcompletionSource;
    }

    public async Task StartAsync(CancellationToken ct)
    {
        var consumers = _processorFactory
            .CreateProcessors(_config)
            .Select(processor => new ConsumerTask(processor));

        await _kafkaInitcompletionSource.Task;
        
        await Parallel.ForEachAsync(consumers, ct, async (consumer, ct) 
            => await consumer.Run(ct));
    }
    
    public Task StopAsync(CancellationToken ct)
        => Task.CompletedTask;
}