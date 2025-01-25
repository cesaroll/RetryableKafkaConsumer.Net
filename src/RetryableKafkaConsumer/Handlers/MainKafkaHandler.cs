using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using RetryableKafkaConsumer.Contracts.Handlers;
using RetryableKafkaConsumer.Contracts.Results;
using RetryableKafkaConsumer.Producers;

namespace RetryableKafkaConsumer.Handlers;

internal class MainKafkaHandler<TKey, TValue> : IHandler<TKey, TValue>
{
    private readonly IHandler<TKey, TValue> _payloadHandler;
    private readonly IEventProducer _retryProducer;
    private readonly IEventProducer _dlqProducer;
    private readonly ILogger<MainKafkaHandler<TKey, TValue>> _logger;
    
    public MainKafkaHandler(
        IHandler<TKey, TValue> payloadHandler, 
        IEventProducer retryProducer, 
        IEventProducer dlqProducer,
        ILoggerFactory loggerFactory)
    {
        _payloadHandler = payloadHandler;
        _retryProducer = retryProducer;
        _dlqProducer = dlqProducer;
        _logger = loggerFactory.CreateLogger<MainKafkaHandler<TKey, TValue>>();
    }

    public string Name { get; }

    public async Task<Result> HandleAsync(ConsumeResult<TKey, TValue> consumeResult, CancellationToken ct)
    {
        try
        {
            var result = await _payloadHandler.HandleAsync(consumeResult, ct);

            if (result is SuccessResult or ErrorResult)
                return result;
            
            if (result is RetryResult)
                return await _retryProducer.ProduceAsync(consumeResult, ct);
            
            if (result is DlqResult)
                return await _dlqProducer.ProduceAsync(consumeResult, ct);

            return new ErrorResult("An unknown error occurred while handling the message");
        }
        catch (Exception ex)
        {
            var msg = "An error occurred while handling the message";
            _logger.LogError(ex, msg);
            return new ErrorResult(msg, ex);
        }
    }
}