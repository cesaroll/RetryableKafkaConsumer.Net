using Confluent.Kafka;
using RetryableKafkaConsumer.Contracts.Results;

namespace RetryableKafkaConsumer.Producers;

internal class DlqEventProducer : IEventProducer
{
    public Task<Result> ProduceAsync<TKey, TValue>(ConsumeResult<TKey, TValue> consumeResult, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}