using Confluent.Kafka;
using RetryableConsumer.Abstractions.Results;

namespace RetryableConsumer.Infra.Kafka.Consumers.Services;

public class ConsumerHandlerService<TKey, TValue> : IConsumerHandlerService<TKey, TValue>
{
    public Task<Result> HandleAsync(ConsumeResult<TKey, TValue> consumeResult, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}