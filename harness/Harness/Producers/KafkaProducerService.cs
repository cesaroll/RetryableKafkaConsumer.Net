using Confluent.Kafka;
using Harness.Producers.Configs;

namespace Harness.Producers;

public class KafkaProducerService<T> : IKafkaProducerService<T>
{
    private readonly IProducer<Null, T> _producer;
    private readonly string _topic;

    public KafkaProducerService(
        ISerializer<T> serializer,
        ProducerServiceConfig producerServiceConfig
        )
    {
        var config = new ProducerConfig
        {
            BootstrapServers = producerServiceConfig.Host
        };
        _producer = new ProducerBuilder<Null, T>(config)
            .SetValueSerializer(serializer)
            .Build();
        _topic = producerServiceConfig.Topic;
    }

    public async Task ProduceAsync(T message)
    {
        var kafkaMessage = new Message<Null, T> { Value = message };
        await _producer.ProduceAsync(_topic, kafkaMessage);
    }
}