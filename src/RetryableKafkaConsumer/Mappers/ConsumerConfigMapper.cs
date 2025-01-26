using Confluent.Kafka;
using RetryableKafkaConsumer.Contracts.Configs;

namespace RetryableKafkaConsumer.Mappers;

internal static class ConsumerConfigMapper
{
    internal static ConsumerConfig ToConsumerConfig(this RetryableConsumerConfig config)
    {
        return new ConsumerConfig
        {
            BootstrapServers = config.Server,
            GroupId = config.GroupId,
            EnableAutoCommit = false,
            AutoOffsetReset = AutoOffsetReset.Earliest
        };
    }
}