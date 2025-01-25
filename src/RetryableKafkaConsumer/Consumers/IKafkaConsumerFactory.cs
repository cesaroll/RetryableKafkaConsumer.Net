using Confluent.Kafka;
using RetryableKafkaConsumer.Contracts.Configs;
using RetryableKafkaConsumer.Contracts.Handlers;

namespace RetryableKafkaConsumer.Consumers;

internal interface IKafkaConsumerFactory<TKey, TValue>
{
    IConsumer<TKey, TValue> CreateKafkaConsumer(RetryableConsumerConfig retryableConsumerConfig);
}