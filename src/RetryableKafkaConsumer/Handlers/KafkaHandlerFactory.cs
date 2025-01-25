using Microsoft.Extensions.Logging;
using RetryableKafkaConsumer.Contracts.Handlers;
using RetryableKafkaConsumer.Producers;

namespace RetryableKafkaConsumer.Handlers;

internal class KafkaHandlerFactory<TKey, TValue> : IKafkaHandlerFactory<TKey, TValue>
{
    private readonly IEventProducer _retryProducer;
    private readonly IEventProducer _dlqProducer;
    private readonly ILogger<KafkaHandlerFactory<TKey, TValue>> _logger;
    private readonly ILoggerFactory _loggerFactory;
    
    public KafkaHandlerFactory(
        IEventProducer retryProducer, 
        IEventProducer dlqProducer, 
        ILoggerFactory loggerFactory)
    {
        _retryProducer = retryProducer;
        _dlqProducer = dlqProducer;
        _loggerFactory = loggerFactory;
        
        _logger = loggerFactory.CreateLogger<KafkaHandlerFactory<TKey, TValue>>();
    }

    public IHandler<TKey, TValue> CreateMainHandler(IHandler<TKey, TValue> payloadHandler)
        => new MainKafkaHandler<TKey, TValue>(
            payloadHandler, _retryProducer, _dlqProducer, _loggerFactory);

    public IHandler<TKey, TValue> CreateRetryHandler(IHandler<TKey, TValue> payloadHandler)
    {
        throw new NotImplementedException();
    }
}