using Microsoft.Extensions.Hosting;
using RetryableKafkaConsumer.Consumers;

namespace RetryableKafkaConsumer.HostedServices;

internal class ConsumerHostedService<TKey, TValue> : BackgroundService
{
    private readonly List<IConsumerTask> _consumers;
    
    public ConsumerHostedService(IConsumerTaskFactory<TKey, TValue> consumerTaskFactory)
    {
        _consumers = consumerTaskFactory.CreateTaskConsumers();
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var tasks = _consumers.Select(consumer => consumer.Run(stoppingToken));
        await Task.Run(() => Task.WhenAll(tasks), stoppingToken);
    }
}