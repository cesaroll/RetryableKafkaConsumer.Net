using RetryableKafkaConsumer.Contracts.Handlers;

namespace RetryableKafkaConsumer.Handlers;

internal interface IKafkaHandlerFactory<TKey, TValue>
{ 
    IHandler<TKey, TValue> CreateMainHandler(IHandler<TKey, TValue> payloadHandler);
    IHandler<TKey, TValue> CreateRetryHandler(IHandler<TKey, TValue> payloadHandler);
}