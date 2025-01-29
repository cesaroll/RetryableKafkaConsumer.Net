using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using RetryableKafkaConsumer.Contracts.Results;

namespace RetryableKafkaConsumer.Producers;

internal class RetryableEventProducer<TKey, TValue> : IEventProducer<TKey, TValue>
{
    private readonly IEventProducer<TKey, TValue> _eventProducer;

    public RetryableEventProducer(IEventProducer<TKey, TValue> eventProducer)
    {
        _eventProducer = eventProducer;
    }

    public Task<Result> ProduceAsync(Message<TKey, TValue> message, CancellationToken ct)
    {
        return _eventProducer.ProduceAsync(message, ct);
    }
}