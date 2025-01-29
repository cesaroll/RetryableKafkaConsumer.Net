using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using RetryableKafkaConsumer.Contracts.Results;

namespace RetryableKafkaConsumer.Producers;

internal class EventProducer<TKey, TValue> : IEventProducer<TKey, TValue>
{
    private readonly IProducer<TKey, TValue> _producer;
    private readonly string _topic;
    private readonly ILogger _logger;
    
    public EventProducer(
        IProducer<TKey, TValue> producer,
        string topic,
        ILoggerFactory loggerFactory)
    {
        _producer = producer;
        _topic = topic;
        _logger = loggerFactory.CreateLogger<EventProducer<TKey, TValue>>();
    }
    
    public async Task<Result> ProduceAsync(Message<TKey, TValue> message, CancellationToken ct)
    {
        try
        {
            await _producer.ProduceAsync(_topic, message, ct); // TODO: Add retries
            _logger.LogInformation($"Produced message to topic: {_topic}");
            return new SuccessResult();
            
        } catch(Exception ex)
        {
            _logger.LogError(ex, "An error occurred while producing message");
            throw;
        }
    }
}