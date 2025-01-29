using Confluent.Kafka;
using RetryableConsumer.Abstractions.Results;

namespace RetryableConsumer.Abstractions.Handlers;

public interface IHandler<TKey, TValue>
{
    public Task<Result> HandleAsync(ConsumeResult<TKey, TValue> consumeResult, CancellationToken ct);
}