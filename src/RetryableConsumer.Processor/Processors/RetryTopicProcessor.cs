using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using RetryableConsumer.Abstractions.Handlers;
using RetryableConsumer.Abstractions.Results;
using RetryableConsumer.Infra.Kafka.Consumers;
using RetryableConsumer.Infra.Kafka.Producers;
using RetryableConsumer.Processor.Extensions;

namespace RetryableConsumer.Processor.Processors;

public class RetryTopicProcessor<TKey, TValue> : Processor<TKey, TValue>
{
    private readonly IProducerWrapper<TKey, TValue>? _currentRetryProducer;

    public RetryTopicProcessor(
        IConsumerWrapper<TKey, TValue> consumer, 
        IHandler<TKey, TValue> payloadHandler, 
        IProducerWrapper<TKey, TValue>? currentRetryProducer,
        IProducerWrapper<TKey, TValue>? retryProducer, 
        IProducerWrapper<TKey, TValue>? dlqProducer, 
        ILogger<Processor<TKey, TValue>> logger) 
        : base(consumer, payloadHandler, retryProducer, dlqProducer, logger)
    {
        _currentRetryProducer = currentRetryProducer;
    }

    protected override async Task BeforeHandleAsync(ConsumeResult<TKey, TValue> consumeResult, CancellationToken ct)
        => await DelayAsNeededAsync(consumeResult.Message, ct);
    
    protected override async Task<Result> TryRetry(Message<TKey, TValue> message, CancellationToken ct)
    {
        var localRetryCount = message.GetLocalRetryCountHeader();
        var overallRetryCount = message.GetOverallRetryCountHeader();
        
        var newMessage = new Message<TKey, TValue>
        {
            Key = message.Key,
            Value = message.Value,
            Headers = message.Headers
        };
        
        newMessage.SetLocalRetryCountHeader(localRetryCount + 1);
        newMessage.SetOverallRetryCountHeader(overallRetryCount + 1);
        
        var producer = GetProducer();
        
        if (producer != null)
            return await producer.ProduceAsync(newMessage, ct);
    
        return await TryDlq(newMessage, ct);

        IProducerWrapper<TKey, TValue>? GetProducer()
        {
            if (localRetryCount < _consumer.RetryAttempts)
                return _currentRetryProducer;
            
            newMessage.SetLocalRetryCountHeader(0);
            
            return _retryProducer;
        }
    }

    private async Task DelayAsNeededAsync(Message<TKey, TValue> message, CancellationToken ct)
    {
        var currentDateTime = DateTime.UtcNow;
        var shouldRunDateTime = message.Timestamp.UtcDateTime.Add(_consumer.RetryDelay!.Value);
        
        if (shouldRunDateTime <= currentDateTime) 
            return;
        
        var delay = shouldRunDateTime - currentDateTime;
        
        _logger.LogInformation($"Delay handling {_consumer.RegistrationId}:{_consumer.Topic} for {delay.ToString()}");
        
        await Task.Delay(delay, ct);
    }
}