using Confluent.Kafka;

namespace RetryableConsumer.Infra.Kafka.Consumers;

public interface IConsumerWrapper<TKey, TValue>
{
    string RegistrationId { get;}
    string Topic { get;}
    public void Subscribe();
    public ConsumeResult<TKey, TValue> Consume(CancellationToken ct);
}