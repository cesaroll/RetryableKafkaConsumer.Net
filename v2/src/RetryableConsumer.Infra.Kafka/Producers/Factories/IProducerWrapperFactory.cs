using RetryableConsumer.Infra.Kafka.Producers.Config;

namespace RetryableConsumer.Infra.Kafka.Producers.Factories;

public interface IProducerWrapperFactory<TKey, TValue>
{
    IProducerWrapper<TKey, TValue> Create(ProducerWrapperConfig config);
}