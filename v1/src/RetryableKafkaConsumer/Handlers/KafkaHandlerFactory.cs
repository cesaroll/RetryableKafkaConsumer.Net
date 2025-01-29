using Microsoft.Extensions.Logging;
using RetryableKafkaConsumer.Contracts.Configs;
using RetryableKafkaConsumer.Contracts.Handlers;
using RetryableKafkaConsumer.Producers;
using RetryableKafkaConsumer.Producers.Config;

namespace RetryableKafkaConsumer.Handlers;

internal class KafkaHandlerFactory<TKey, TValue> : IKafkaHandlerFactory<TKey, TValue>
{
    private readonly IProducerFactory<TKey, TValue> _producerFactory;
    private readonly ILogger<KafkaHandlerFactory<TKey, TValue>> _logger;
    private readonly ILoggerFactory _loggerFactory;
    
    public KafkaHandlerFactory(
        IProducerFactory<TKey, TValue> producerFactory,
        ILoggerFactory loggerFactory)
    {   
        _producerFactory = producerFactory;
        _loggerFactory = loggerFactory;
        
        _logger = loggerFactory.CreateLogger<KafkaHandlerFactory<TKey, TValue>>();
    }

    public IHandler<TKey, TValue> CreateMainHandler(
        IHandler<TKey, TValue> payloadHandler, 
        RetryableConsumerConfig retryableConsumerConfig)
    {
        return new MainKafkaHandler<TKey, TValue>(
            payloadHandler, 
            CreateFirstRetryProducer(retryableConsumerConfig), 
            null, // TODO: use separate producer for dead-letter 
            _loggerFactory);
    }

    public IHandler<TKey, TValue> CreateRetryHandler(
        IHandler<TKey, TValue> payloadHandler, 
        RetryableConsumerConfig retryableConsumerConfig)
    {
        throw new NotImplementedException();
    }

    private IEventProducer<TKey, TValue>? CreateFirstRetryProducer(RetryableConsumerConfig config)
        => config.Retries.FirstOrDefault() != null 
            ? CreateRetryProducer(config.Retries.FirstOrDefault()!.Topic, config)
            : null;
    
    private IEventProducer<TKey, TValue>? CreateRetryProducer(string topic, RetryableConsumerConfig config)
    {
        var retryConfig = config.Retries.FirstOrDefault(x => x.Topic == topic);

        if (retryConfig is null)
            return null;
        
        var server = retryConfig.Server?? config.Server;
        
        return _producerFactory.CreateProducer(
            new ProducerConfig(server, topic));
    }
}