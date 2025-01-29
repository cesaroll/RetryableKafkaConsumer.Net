using RetryableConsumer.Infra.Kafka.Consumers.Services;

namespace RetryableConsumer.Infra.Kafka.Consumers.Factories;

public interface IConsumerHandlerServiceFactory<TKey, TValue>
{
    IConsumerHandlerService<TKey, TValue> Create();
}