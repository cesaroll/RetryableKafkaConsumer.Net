using RetryableKafkaConsumer.Contracts.Configs;
using RetryableKafkaConsumer.Contracts.Handlers;

namespace RetryableKafkaConsumer.Handlers;

internal interface IKafkaHandlerFactory<TKey, TValue>
{ 
    IHandler<TKey, TValue> CreateMainHandler(IHandler<TKey, TValue> payloadHandler, RetryableConsumerConfig retryableConsumerConfig);
    IHandler<TKey, TValue> CreateRetryHandler(IHandler<TKey, TValue> payloadHandler, RetryableConsumerConfig retryableConsumerConfig);
}