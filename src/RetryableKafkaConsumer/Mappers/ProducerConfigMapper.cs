using Confluent.Kafka;
using RetryableKafkaConsumer.Producers;

namespace RetryableKafkaConsumer.Mappers;

internal static class ProducerConfigMapper
{
    internal static ProducerConfig ToProducerConfig(this RetryableProducerConfig config)
    {
        return new ProducerConfig
        {
            BootstrapServers = config.Server
        };
    }
}