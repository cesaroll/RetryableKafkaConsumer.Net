using RetryableKafkaConsumer.Contracts.Configs;
using RetryableKafkaConsumer.Contracts.Handlers;

namespace RetryableKafkaConsumer.Consumers;

internal interface IConsumerTaskFactory<TKey, TValue>
{
    List<IConsumerTask> CreateTaskConsumers();
    // IConsumerTask CreateMainTaskConsumer(RetryableConsumerConfig retryableConsumerConfig, IHandler<TKey, TValue> payloadHandler);
    // IConsumerTask CreateRetryTaskConsumer(RetryableConsumerConfig retryableConsumerConfig, IHandler<TKey, TValue> payloadHandler);
}