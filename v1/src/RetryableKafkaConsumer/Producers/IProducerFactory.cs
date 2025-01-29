using RetryableKafkaConsumer.Producers.Config;

namespace RetryableKafkaConsumer.Producers;

internal interface IProducerFactory<TKey, TValue>
{
    IEventProducer<TKey, TValue> CreateProducer(ProducerConfig producerConfig);
}