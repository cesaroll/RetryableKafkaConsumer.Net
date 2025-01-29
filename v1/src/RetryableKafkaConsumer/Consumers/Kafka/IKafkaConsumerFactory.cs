using Confluent.Kafka;

namespace RetryableKafkaConsumer.Consumers.Kafka;

internal interface IKafkaConsumerFactory<TKey, TValue>
{
    IConsumer<TKey, TValue> CreateKafkaConsumer(ConsumerConfig retryableConsumerConfig);
}