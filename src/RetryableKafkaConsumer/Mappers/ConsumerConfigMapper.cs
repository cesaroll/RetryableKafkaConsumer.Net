using Confluent.Kafka;
using RetryableKafkaConsumer.Contracts.Configs;

namespace RetryableKafkaConsumer.Mappers;

public static class ConsumerConfigMapper
{
    public static ConsumerConfig ToConsumerConfig(this RetryableConsumerConfig config)
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