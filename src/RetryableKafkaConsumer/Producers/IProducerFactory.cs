namespace RetryableKafkaConsumer.Producers;

internal interface IProducerFactory<TKey, TValue>
{
    IEventProducer<TKey, TValue> CreateRetryProducer(RetryableProducerConfig retryableProducerConfig);
}