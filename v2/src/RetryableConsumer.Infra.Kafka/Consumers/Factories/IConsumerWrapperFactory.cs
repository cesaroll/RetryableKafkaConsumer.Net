using RetryableConsumer.Infra.Kafka.Consumers.Config;

namespace RetryableConsumer.Infra.Kafka.Consumers.Factories;

public interface IConsumerWrapperFactory<TKey, TValue>
{
    IConsumerWrapper<TKey, TValue> Create(ConsumerWrapperConfig config);
}