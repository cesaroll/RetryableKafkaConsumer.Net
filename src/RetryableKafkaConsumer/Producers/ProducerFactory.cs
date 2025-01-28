using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using RetryableKafkaConsumer.Mappers;
using ProducerConfig = RetryableKafkaConsumer.Producers.Config.ProducerConfig;

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

    public IEventProducer<TKey, TValue> CreateProducer(ProducerConfig producerConfig)
    {
        var producer = new ProducerBuilder<TKey, TValue>(producerConfig.ToProducerConfig())
            .SetKeySerializer(_keySerializer)
            .SetValueSerializer(_valueSerializer)
            .Build();
        
        return new EventProducer<TKey, TValue>(producer, producerConfig.Topic, _loggerFactory);
    }
}