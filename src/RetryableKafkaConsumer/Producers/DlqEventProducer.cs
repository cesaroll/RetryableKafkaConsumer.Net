using Confluent.Kafka;
using Microsoft.Extensions.Logging;

namespace RetryableKafkaConsumer.Producers;

internal class DlqEventProducer<TKey, TValue> : EventProducer<TKey, TValue>
{
    public DlqEventProducer(
        IProducer<TKey, TValue> producer, string topic, ILoggerFactory loggerFactory) : 
        base(producer, topic, loggerFactory)
    {
    }

    protected override ILogger CreateLogger(ILoggerFactory loggerFactory)
        => loggerFactory.CreateLogger<DlqEventProducer<TKey, TValue>>();
}