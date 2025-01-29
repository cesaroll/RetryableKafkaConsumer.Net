using Confluent.Kafka;

namespace RetryableConsumer.Infra.Kafka.Consumers;

public interface IConsumerWrapper<TKey, TValue>
{
    string RegistrationId { get;}
    string Topic { get;}
    TimeSpan? RetryDelay { get;}
    int? RetryAttempts { get;}
    public void Subscribe();
    public ConsumeResult<TKey, TValue> Consume(CancellationToken ct);
    public void Commit();
    public void Close();
}