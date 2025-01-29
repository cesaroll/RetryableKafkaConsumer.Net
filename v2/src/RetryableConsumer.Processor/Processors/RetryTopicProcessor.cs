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

        var producer = GetProducer();
        
        message.SetLocalRetryCountHeader(localRetryCount + 1);
        message.SetOverallRetryCountHeader(overallRetryCount + 1);
        
        if (producer != null)
            return await producer.ProduceAsync(message, ct);
    
        return await TryDlq(message, ct);

        IProducerWrapper<TKey, TValue>? GetProducer()
        {
            if (localRetryCount < _consumer.RetryAttempts)
                return _currentRetryProducer;
            
            return _retryProducer;
        }
    }

    private Task DelayAsNeededAsync(Message<TKey, TValue> message, CancellationToken ct)
    {
        var currentDateTime = DateTime.UtcNow;
        var shouldRunDateTime = message.Timestamp.UtcDateTime.Add(_consumer.RetryDelay!.Value);
        
        if (shouldRunDateTime <= currentDateTime) 
            return Task.CompletedTask;
        
        var delay = shouldRunDateTime - currentDateTime;
        
        _logger.LogInformation($"Delay handling {_consumer.RegistrationId}:{_consumer.Topic} for {delay.ToString()}");
        
        return Task.Delay(delay, ct);
    }
}