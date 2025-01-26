using Microsoft.Extensions.Hosting;
using RetryableKafkaConsumer.Consumers;

namespace RetryableKafkaConsumer.HostedServices;

internal class ConsumerHostedService<TKey, TValue> : IHostedService
{
    private readonly List<IConsumerTask> _consumers;
    
    public ConsumerHostedService(IConsumerTaskFactory consumerTaskFactory)
    {
        _consumers = consumerTaskFactory.CreateTaskConsumers();
    }

    public async Task StartAsync(CancellationToken ct)
    {
        var tasks = _consumers.Select(consumer => consumer.Run(ct));
        await Task.Run(() => Task.WhenAll(tasks), ct);
    }

    public Task StopAsync(CancellationToken cancellationToken)
        => Task.CompletedTask;
}