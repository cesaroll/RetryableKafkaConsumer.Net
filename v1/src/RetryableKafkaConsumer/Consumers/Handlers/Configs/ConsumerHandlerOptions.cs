using RetryableKafkaConsumer.Contracts.Handlers;
using RetryableKafkaConsumer.Producers;

namespace RetryableKafkaConsumer.Consumers.Handlers.Configs;

internal record ConsumerHandlerOptions<TKey, TValue>(
    int MaxInTopicRetryAttempts,
    IEventProducer<TKey, TValue>? CurrentProducer, 
    IEventProducer<TKey, TValue>? NextProducer, 
    IEventProducer<TKey, TValue>? DlqProducer
    );