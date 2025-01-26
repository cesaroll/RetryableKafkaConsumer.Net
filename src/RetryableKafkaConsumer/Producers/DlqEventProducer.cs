using Confluent.Kafka;
using RetryableKafkaConsumer.Contracts.Results;

namespace RetryableKafkaConsumer.Producers;

internal class DlqEventProducer<TKey, TValue> : IEventProducer<TKey, TValue>
{
    public Task<Result> ProduceAsync(ConsumeResult<TKey, TValue> consumeResult, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}