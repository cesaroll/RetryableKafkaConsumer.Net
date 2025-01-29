using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using RetryableConsumer.Abstractions.Handlers;
using RetryableConsumer.Abstractions.Results;
using RetryableConsumer.Infra.Kafka.Consumers;
using RetryableConsumer.Infra.Kafka.Producers;
using RetryableConsumer.Processor.Extensions;

namespace RetryableConsumer.Processor.Processors;

public class MainTopicProcessor<TKey, TValue> : Processor<TKey, TValue>
{
    public MainTopicProcessor(
        IConsumerWrapper<TKey, TValue> consumer, 
        IHandler<TKey, TValue> payloadHandler, 
        IProducerWrapper<TKey, TValue>? retryProducer, 
        IProducerWrapper<TKey, TValue>? dlqProducer, 
        ILogger<Processor<TKey, TValue>> logger) : 
        base(consumer, payloadHandler, retryProducer, dlqProducer, logger)
    {
    }

    protected override Task BeforeHandleAsync(ConsumeResult<TKey, TValue> consumeResult, CancellationToken ct)
        => Task.CompletedTask;
    
    protected override async Task<Result> TryRetry(Message<TKey, TValue> message, CancellationToken ct)
    {
        var newMessage = new Message<TKey, TValue>
        {
            Key = message.Key,
            Value = message.Value,
            Headers = message.Headers
        };
        
        newMessage.SetLocalRetryCountHeader(1);
        newMessage.SetOverallRetryCountHeader(1);
        
        if (_retryProducer != null)
            return await _retryProducer.ProduceAsync(newMessage, ct);
    
        return await TryDlq(newMessage, ct);
    }
}