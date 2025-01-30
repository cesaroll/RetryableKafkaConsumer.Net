using Confluent.Kafka;

namespace RetryableConsumer.Infra.Kafka.Consumers;

public class ConsumerWrapper<TKey, TValue> : IConsumerWrapper<TKey, TValue>
{
    private readonly IConsumer<TKey, TValue> _consumer;

    public string RegistrationId { get; }
    public string Topic { get; }
    public TimeSpan? RetryDelay { get; }
    public int? RetryAttempts { get; }
    
    public ConsumerWrapper(
        string registrationId, 
        string topic,
        TimeSpan? retryDelay,
        int? retryAttempts,
        IConsumer<TKey, TValue> consumer)
    {
        RegistrationId = registrationId;
        Topic = topic;
        RetryDelay = retryDelay;
        RetryAttempts = retryAttempts;
        _consumer = consumer;
    }
    
    public void Subscribe()
        => _consumer.Subscribe(Topic);
    
    public async Task<ConsumeResult<TKey, TValue>> ConsumeAsync(CancellationToken ct)
        => await Task.Run(() => _consumer.Consume(ct), ct);
    
    public void Commit()
        => _consumer.Commit();
    
    public void Close()
        => _consumer.Close();
}