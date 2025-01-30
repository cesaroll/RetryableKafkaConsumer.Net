using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using RetryableConsumer.Infra.Kafka.Producers.Config;

namespace RetryableConsumer.Infra.Kafka.Producers.Factories;

public class ProducerWrapperFactory<TKey, TValue> : IProducerWrapperFactory<TKey, TValue>
{
    private readonly ISerializer<TKey> _keySerializer;
    private readonly ISerializer<TValue> _valueSerializer;
    private readonly ILoggerFactory _loggerFactory;

    public ProducerWrapperFactory(
        ISerializer<TKey> keySerializer, 
        ISerializer<TValue> valueSerializer, 
        ILoggerFactory loggerFactory)
    {
        _keySerializer = keySerializer;
        _valueSerializer = valueSerializer;
        _loggerFactory = loggerFactory;
    }

    public IProducerWrapper<TKey, TValue> Create(ProducerWrapperConfig config)
    {
        var builder = new ProducerBuilder<TKey, TValue>(new ProducerConfig()
            {
                BootstrapServers = config.Host,
                MessageSendMaxRetries = 3
            })
            .SetKeySerializer(_keySerializer)
            .SetValueSerializer(_valueSerializer);
        
        var logger = _loggerFactory.CreateLogger<ProducerWrapper<TKey, TValue>>();

        return new ProducerWrapper<TKey, TValue>(
            config.RegistrationId, 
            config.Topic, 
            builder.Build(), 
            logger
            );
    }

}