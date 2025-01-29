using RetryableConsumer.Abstractions.Handlers;

namespace RetryableConsumer.Infra.Kafka.Consumers.Services;

public interface IConsumerHandlerService<TKey, TValue> : IHandler<TKey, TValue>
{
}