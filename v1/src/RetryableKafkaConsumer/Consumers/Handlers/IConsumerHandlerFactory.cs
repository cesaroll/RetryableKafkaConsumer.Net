using RetryableKafkaConsumer.Contracts.Handlers;

namespace RetryableKafkaConsumer.Consumers.Handlers;

public interface IConsumerHandlerFactory<TKey, TValue>
{
    IHandler<TKey, TValue> CreateHandler();
}