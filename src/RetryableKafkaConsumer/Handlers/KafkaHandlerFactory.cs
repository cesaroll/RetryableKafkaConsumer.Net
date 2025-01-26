using Microsoft.Extensions.Logging;
using RetryableKafkaConsumer.Contracts.Handlers;
using RetryableKafkaConsumer.Producers;

namespace RetryableKafkaConsumer.Handlers;

internal class KafkaHandlerFactory<TKey, TValue> : IKafkaHandlerFactory<TKey, TValue>
{
    // private readonly IEventProducer<TKey, TValue> _retryProducer;
    // private readonly IEventProducer<TKey, TValue> _dlqProducer;
    
    private readonly IProducerFactory<TKey, TValue> _producerFactory;
    private readonly ILogger<KafkaHandlerFactory<TKey, TValue>> _logger;
    private readonly ILoggerFactory _loggerFactory;
    
    public KafkaHandlerFactory(
        // IEventProducer<TKey, TValue> retryProducer, 
        // IEventProducer<TKey, TValue> dlqProducer, 
        IProducerFactory<TKey, TValue> producerFactory,
        ILoggerFactory loggerFactory)
    {
        // _retryProducer = retryProducer;
        // _dlqProducer = dlqProducer;
        
        _producerFactory = producerFactory;
        _loggerFactory = loggerFactory;
        
        _logger = loggerFactory.CreateLogger<KafkaHandlerFactory<TKey, TValue>>();
    }

    public IHandler<TKey, TValue> CreateMainHandler(IHandler<TKey, TValue> payloadHandler)
    {
        var retryProducer = _producerFactory.CreateRetryProducer(
            new RetryableProducerConfig("localhost:9092", "test-topic-retry1"));
        
        return new MainKafkaHandler<TKey, TValue>(
            payloadHandler, retryProducer, retryProducer, _loggerFactory);
    }

    public IHandler<TKey, TValue> CreateRetryHandler(IHandler<TKey, TValue> payloadHandler)
    {
        throw new NotImplementedException();
    }
}