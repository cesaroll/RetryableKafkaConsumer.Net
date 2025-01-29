using Confluent.Kafka;
using Microsoft.Extensions.Logging;

namespace RetryableConsumer.Infra.Kafka.Consumers;

public class ConsumerWrapper<TKey, TValue> : IConsumerWrapper<TKey, TValue>
{
    private readonly IConsumer<TKey, TValue> _consumer;

    public string RegistrationId { get; }
    public string Topic { get; }
    
    public ConsumerWrapper(
        string registrationId, 
        string topic,
        IConsumer<TKey, TValue> consumer)
    {
        RegistrationId = registrationId;
        Topic = topic;
        _consumer = consumer;
    }
    
    public void Subscribe()
        => _consumer.Subscribe(Topic);

    public ConsumeResult<TKey, TValue> Consume(CancellationToken ct)
        => _consumer.Consume(ct);
}