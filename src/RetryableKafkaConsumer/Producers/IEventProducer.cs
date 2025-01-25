using Confluent.Kafka;
using RetryableKafkaConsumer.Contracts.Results;

namespace RetryableKafkaConsumer.Producers;

internal interface IEventProducer
{
    Task<Result> ProduceAsync<TKey, TValue>(ConsumeResult<TKey, TValue> consumeResult, CancellationToken ct);
}