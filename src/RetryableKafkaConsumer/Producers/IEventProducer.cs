using Confluent.Kafka;
using RetryableKafkaConsumer.Contracts.Results;

namespace RetryableKafkaConsumer.Producers;

internal interface IEventProducer<TKey, TValue>
{
    Task<Result> ProduceAsync(Message<TKey, TValue> message, CancellationToken ct);
}