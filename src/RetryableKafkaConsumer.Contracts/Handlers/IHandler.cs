using Confluent.Kafka;
using RetryableKafkaConsumer.Contracts.Results;

namespace RetryableKafkaConsumer.Contracts.Handlers;

public interface IHandler<TKey, TValue>
{
    //public string Name { get; }
    public Task<Result> HandleAsync(ConsumeResult<TKey, TValue> consumeResult, CancellationToken ct);
}