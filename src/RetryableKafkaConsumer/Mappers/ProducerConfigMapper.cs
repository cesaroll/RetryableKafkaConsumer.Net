using Confluent.Kafka;
using RetryableKafkaConsumer.Producers;
using ProducerConfig = RetryableKafkaConsumer.Producers.Config.ProducerConfig;

namespace RetryableKafkaConsumer.Mappers;

internal static class ProducerConfigMapper
{
    internal static Confluent.Kafka.ProducerConfig ToProducerConfig(this ProducerConfig config)
    {
        return new Confluent.Kafka.ProducerConfig
        {
            BootstrapServers = config.Server
        };
    }
}