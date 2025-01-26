using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using RetryableKafkaConsumer.Contracts.Results;
using RetryableKafkaConsumer.Mappers;

namespace RetryableKafkaConsumer.Producers;

internal abstract class EventProducer<TKey, TValue> : IEventProducer<TKey, TValue>
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
        _logger = CreateLogger(loggerFactory);
    }
    
    protected abstract ILogger CreateLogger(ILoggerFactory loggerFactory);
    
    public async Task<Result> ProduceAsync(ConsumeResult<TKey, TValue> consumeResult, CancellationToken ct)
    {
        var message = consumeResult.ToMessage();
        
        message.Headers = AddHeaders(consumeResult);
        
        try
        {
            await _producer.ProduceAsync(_topic, message, ct);
            _logger.LogInformation($"Produced message to topic: {_topic}");
            return new SuccessResult();
            
        } catch(Exception ex)
        {
            _logger.LogError(ex, "An error occurred while producing message");
            throw;
        }
    }

    protected virtual Headers AddHeaders(ConsumeResult<TKey, TValue> consumeResult)
        => consumeResult.Message.Headers;
}