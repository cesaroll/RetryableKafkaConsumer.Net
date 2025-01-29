using RetryableKafkaConsumer.Contracts.Configs;
using RetryableKafkaConsumer.Contracts.Handlers;

namespace RetryableKafkaConsumer.Consumers;

internal interface IConsumerTaskFactory
{
    List<IConsumerTask> CreateTaskConsumers();
}