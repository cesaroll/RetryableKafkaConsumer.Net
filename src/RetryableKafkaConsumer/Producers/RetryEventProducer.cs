using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using RetryableKafkaConsumer.Contracts.Results;

namespace RetryableKafkaConsumer.Producers;

internal class RetryEventProducer<TKey, TValue> : EventProducer<TKey, TValue>
{
    public RetryEventProducer(
        IProducer<TKey, TValue> producer, string topic, ILoggerFactory loggerFactory) : 
        base(producer, topic, loggerFactory)
    {
    }

    protected override ILogger CreateLogger(ILoggerFactory loggerFactory)
        => loggerFactory.CreateLogger<RetryEventProducer<TKey, TValue>>();
}