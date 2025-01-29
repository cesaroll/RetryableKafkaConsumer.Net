using Microsoft.Extensions.Logging;
using RetryableKafkaConsumer.Contracts.Handlers;
using RetryableKafkaConsumer.Producers;

namespace RetryableKafkaConsumer.Consumers.Handlers;

internal class ConsumerHandlerFactory<TKey, TValue> : IConsumerHandlerFactory<TKey, TValue>
{
    private readonly IProducerFactory<TKey, TValue> _producerFactory;
    private readonly ILogger<ConsumerHandlerFactory<TKey, TValue>> _logger;
    private readonly ILoggerFactory _loggerFactory;
    
    public ConsumerHandlerFactory(
        IProducerFactory<TKey, TValue> producerFactory,
        ILoggerFactory loggerFactory)
    {   
        _producerFactory = producerFactory;
        _loggerFactory = loggerFactory;
        
        _logger = loggerFactory.CreateLogger<ConsumerHandlerFactory<TKey, TValue>>();
    }
    
    public IHandler<TKey, TValue> CreateHandler()
    {
        throw new NotImplementedException();
    }
}