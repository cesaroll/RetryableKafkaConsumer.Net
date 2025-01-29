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
        var consumer = new ConsumerBuilder<TKey, TValue>(new ConsumerConfig()
            {
                BootstrapServers = config.Host,
                GroupId = config.GroupId,
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false
            })
            .SetKeyDeserializer(_keyDeserializer)
            .SetValueDeserializer(_valueDeserializer)
            .Build();
        
        return new ConsumerWrapper<TKey, TValue>(config.RegistrationId, config.Topic, consumer);
    }
}