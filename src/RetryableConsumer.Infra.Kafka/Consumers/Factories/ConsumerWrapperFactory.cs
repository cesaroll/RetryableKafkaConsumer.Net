using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using RetryableConsumer.Infra.Kafka.Consumers.Config;

namespace RetryableConsumer.Infra.Kafka.Consumers.Factories;

public class ConsumerWrapperFactory<TKey, TValue> : IConsumerWrapperFactory<TKey, TValue>
{
    private readonly IDeserializer<TKey> _keyDeserializer;
    private readonly IDeserializer<TValue> _valueDeserializer;

    public ConsumerWrapperFactory(
        IDeserializer<TKey> keyDeserializer, 
        IDeserializer<TValue> valueDeserializer)
    {
        _keyDeserializer = keyDeserializer;
        _valueDeserializer = valueDeserializer;
    }
    
    public IConsumerWrapper<TKey, TValue> Create(ConsumerWrapperConfig config)
    {
        var builder = new ConsumerBuilder<TKey, TValue>(new ConsumerConfig()
            {
                BootstrapServers = config.Host,
                GroupId = config.GroupId,
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false
            })
            .SetValueDeserializer(_valueDeserializer);

        if (_keyDeserializer is not IDeserializer<Ignore>)
            builder.SetKeyDeserializer(_keyDeserializer);
            
        var consumer = builder.Build();
        
        return new ConsumerWrapper<TKey, TValue>(
            config.RegistrationId, 
            config.Topic, 
            config.RetryDelay, 
            config.RetryAttempts, 
            builder.Build()
            );
    }
}