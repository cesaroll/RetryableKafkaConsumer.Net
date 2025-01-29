using Confluent.Kafka;

namespace RetryableConsumer.Infra.Kafka.Producers.Strategy;

public interface IProducerStrategy<TKey, TValue>
{
    IProducerWrapper<TKey, TValue>? GetProducer(string registrationId, string topic);
}