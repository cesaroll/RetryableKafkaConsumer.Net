using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using RetryableKafkaConsumer.Consumers.Handlers.Configs;
using RetryableKafkaConsumer.Contracts.Handlers;
using RetryableKafkaConsumer.Contracts.Results;
using RetryableKafkaConsumer.Producers;

namespace RetryableKafkaConsumer.Consumers.Handlers;

internal class ConsumerHandler<TKey, TValue> : IHandler<TKey, TValue>
{
    private readonly int MaxInTopicRetryCount = 0;
    private readonly int MaxInTopicRetryAttempts = 1;
    
    private readonly IHandler<TKey, TValue> _payloadHandler;
    private readonly ILogger<ConsumerHandler<TKey, TValue>> _logger;
    
    private readonly IEventProducer<TKey, TValue>? _currentProducer;
    private readonly IEventProducer<TKey, TValue>? _nextProducer;
    private readonly IEventProducer<TKey, TValue>? _dlqProducer;

    public ConsumerHandler(
        IHandler<TKey, TValue> payloadHandler,
        ILoggerFactory loggerFactory,
        ConsumerHandlerOptions<TKey, TValue>? options)
    {
        _payloadHandler = payloadHandler;
        _logger = loggerFactory.CreateLogger<ConsumerHandler<TKey, TValue>>();
        
        if (options is not null)
        {
            MaxInTopicRetryAttempts = options.MaxInTopicRetryAttempts;
            _currentProducer = options.CurrentProducer;
            _nextProducer = options.NextProducer;
            _dlqProducer = options.DlqProducer;
        }
    }

    public async Task<Result> HandleAsync(ConsumeResult<TKey, TValue> consumeResult, CancellationToken ct)
    {
        try
        {
            var result = await TryHandleAsync(consumeResult, ct);
            
            if (result is RetryResult)
                return await TryProduceAsync(result, consumeResult.Message, ct);

            return result;

        } catch (Exception ex)
        {
            var msg = "An error occurred while handling the message";
            _logger.LogError(ex, msg);
            throw;
        }
    }

    private async Task<Result> TryHandleAsync(ConsumeResult<TKey, TValue> consumeResult, CancellationToken ct)
    {
        try
        {
            var result = await _payloadHandler.HandleAsync(consumeResult, ct);
            
            if (result is SuccessResult)
                return result;
            
            return await TryProduceAsync(result, consumeResult.Message, ct);
        }
        catch (Exception ex)
        {
            var msg = "An error occurred while handling the message";
            _logger.LogError(ex, msg);
            return new RetryResult(msg, ex);
        }
    }

    private async Task<Result> TryProduceAsync(Result result, Message<TKey, TValue> message, CancellationToken ct)
    {
        var producer = GetProducer(result); // TODO: local retries

        if (producer is null)
            return result;

        return await producer.ProduceAsync(message, ct);
    }
    
    private IEventProducer<TKey, TValue>? GetProducer(Result result)
        => result switch
        {
            RetryResult => _currentProducer?? _nextProducer,
            DlqResult => _dlqProducer,
            _ => null
        };
}