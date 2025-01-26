using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using RetryableKafkaConsumer.Mappers;

namespace RetryableKafkaConsumer.Producers;

internal class ProducerFactory<TKey, TValue> : IProducerFactory<TKey, TValue>
{
    private readonly ISerializer<TKey> _keySerializer;
    private readonly ISerializer<TValue> _valueSerializer;

    private readonly ILoggerFactory _loggerFactory;

    public ProducerFactory(
        ISerializer<TKey> keySerializer,
        ISerializer<TValue> valueSerializer,
        ILoggerFactory loggerFactory)
    {
        _keySerializer = keySerializer;
        _valueSerializer = valueSerializer;
        _loggerFactory = loggerFactory;
    }

    public IEventProducer<TKey, TValue> CreateRetryProducer(RetryableProducerConfig retryableProducerConfig)
    {
        var producer = new ProducerBuilder<TKey, TValue>(retryableProducerConfig.ToProducerConfig())
            .SetKeySerializer(_keySerializer)
            .SetValueSerializer(_valueSerializer)
            .Build();
        
        return new RetryEventProducer<TKey, TValue>(producer, retryableProducerConfig.Topic, _loggerFactory);
    }
}