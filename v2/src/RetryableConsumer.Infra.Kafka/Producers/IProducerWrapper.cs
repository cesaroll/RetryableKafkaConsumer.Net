using Confluent.Kafka;
using RetryableConsumer.Abstractions.Results;

namespace RetryableConsumer.Infra.Kafka.Producers;

public interface IProducerWrapper<TKey, TValue>
{
    string RegistrationId { get;}
    string Topic { get;}
    public Task<Result> ProduceAsync(Message<TKey, TValue> message, CancellationToken ct);
}