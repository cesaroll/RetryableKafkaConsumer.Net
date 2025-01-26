using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using RetryableKafkaConsumer.Contracts.Handlers;
using RetryableKafkaConsumer.Contracts.Results;
using RetryableKafkaConsumer.Producers;

namespace RetryableKafkaConsumer.Handlers;

internal class MainKafkaHandler<TKey, TValue> : IHandler<TKey, TValue>
{
    private readonly IHandler<TKey, TValue> _payloadHandler;
    private readonly IEventProducer<TKey, TValue>? _retryProducer;
    private readonly IEventProducer<TKey, TValue>? _dlqProducer;
    private readonly ILogger<MainKafkaHandler<TKey, TValue>> _logger;
    
    public MainKafkaHandler(
        IHandler<TKey, TValue> payloadHandler, 
        IEventProducer<TKey, TValue>? retryProducer, 
        IEventProducer<TKey, TValue>? dlqProducer,
        ILoggerFactory loggerFactory)
    {
        _payloadHandler = payloadHandler;
        _retryProducer = retryProducer;
        _dlqProducer = dlqProducer;
        _logger = loggerFactory.CreateLogger<MainKafkaHandler<TKey, TValue>>();
    }

    public async Task<Result> HandleAsync(ConsumeResult<TKey, TValue> consumeResult, CancellationToken ct)
    {
        try
        {
            var result = await TryHanldeAsync(consumeResult, ct);
            
            switch (result)
            {
                case RetryResult when _retryProducer != null:
                    return await _retryProducer.ProduceAsync(consumeResult, ct);
                default:
                    return result;
            }
        }
        catch (Exception ex)
        {
            var msg = "An error occurred while handling the message";
            _logger.LogError(ex, msg);
            throw;
        }
    }

    private async Task<Result> TryHanldeAsync(ConsumeResult<TKey, TValue> consumeResult, CancellationToken ct)
    {
        try
        {
            var result = await _payloadHandler.HandleAsync(consumeResult, ct);
            
            switch (result)
            {
                case SuccessResult:
                    return result;
                case RetryResult when _retryProducer != null:
                    return await _retryProducer.ProduceAsync(consumeResult, ct);
                case DlqResult when _dlqProducer != null:
                    return await _dlqProducer.ProduceAsync(consumeResult, ct);
                case DlqResult:
                    return result;
            }
        }
        catch (Exception ex)
        {
            var msg = "An error occurred while handling the message";
            _logger.LogError(ex, msg);
            return new RetryResult(msg, ex);
        }

        return new RetryResult();
    }
}