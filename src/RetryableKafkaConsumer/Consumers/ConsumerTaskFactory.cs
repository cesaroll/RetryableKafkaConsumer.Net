using Microsoft.Extensions.Logging;
using RetryableKafkaConsumer.Contracts.Configs;
using RetryableKafkaConsumer.Contracts.Handlers;
using RetryableKafkaConsumer.Handlers;

namespace RetryableKafkaConsumer.Consumers;

internal class ConsumerTaskFactory<TKey, TValue> : IConsumerTaskFactory<TKey, TValue>
{
    private readonly RetryableConsumerConfig _retryableConsumerConfig;
    private readonly IHandler<TKey, TValue> _payloadHandler;
    private readonly IKafkaConsumerFactory<TKey, TValue> _kafkaConsumerFactory;
    private readonly IKafkaHandlerFactory<TKey, TValue> _kafkaHandlerFactory;
    private readonly ILoggerFactory _loggerFactory;
    
    private readonly ILogger _logger;

    public ConsumerTaskFactory(
        RetryableConsumerConfig retryableConsumerConfig,
        IHandler<TKey, TValue> payloadHandler,
        IKafkaConsumerFactory<TKey, TValue> kafkaConsumerFactory, 
        IKafkaHandlerFactory<TKey, TValue> kafkaHandlerFactory,
        ILoggerFactory loggerFactory)
    {
        _retryableConsumerConfig = retryableConsumerConfig;
        _payloadHandler = payloadHandler;
        _kafkaConsumerFactory = kafkaConsumerFactory;
        _kafkaHandlerFactory = kafkaHandlerFactory;
        _loggerFactory = loggerFactory;
        _logger = loggerFactory.CreateLogger<ConsumerTaskFactory<TKey, TValue>>();
    }

    public List<IConsumerTask> CreateTaskConsumers()
    {
        var consumers = new List<IConsumerTask>();

        consumers.Add(CreateMainTaskConsumer());
        
        return consumers;
    }
    
    private IConsumerTask CreateMainTaskConsumer()
    {
        var kafkaConsumer = _kafkaConsumerFactory.CreateKafkaConsumer(_retryableConsumerConfig);
        var kafkaHandler = _kafkaHandlerFactory.CreateMainHandler(_payloadHandler);
    
        return new ConsumerTask<TKey, TValue>(
            _retryableConsumerConfig.Topic,
            kafkaConsumer,
            kafkaHandler,
            _loggerFactory);
    }
    
    // private IConsumerTask CreateMainTaskConsumer(
    //     RetryableConsumerConfig retryableConsumerConfig, 
    //     IHandler<TKey, TValue> payloadHandler)
    // {
    //     var kafkaConsumer = _kafkaConsumerFactory.CreateKafkaConsumer(retryableConsumerConfig);
    //     var kafkaHandler = _kafkaHandlerFactory.CreateMainHandler(payloadHandler);
    //
    //     return new ConsumerTask<TKey, TValue>(
    //         retryableConsumerConfig.Topic,
    //         kafkaConsumer,
    //         kafkaHandler,
    //         _loggerFactory);
    // }

    private IConsumerTask CreateRetryTaskConsumer(
        RetryableConsumerConfig retryableConsumerConfig, 
        IHandler<TKey, TValue> payloadHandler)
    {
        throw new NotImplementedException();
    }
}