using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using RetryableKafkaConsumer.Consumers.Config;
using RetryableKafkaConsumer.Consumers.Handlers;
using RetryableKafkaConsumer.Consumers.Kafka;
using RetryableKafkaConsumer.Contracts.Configs;
using RetryableKafkaConsumer.Contracts.Handlers;
using RetryableKafkaConsumer.Handlers;

namespace RetryableKafkaConsumer.Consumers;

internal class ConsumerTaskFactory<TKey, TValue> : IConsumerTaskFactory
{
    private readonly RetryableConsumerConfig _retryableConsumerConfig;
    private readonly IHandler<TKey, TValue> _payloadHandler;
    // private readonly IKafkaConsumerFactory<TKey, TValue> _kafkaConsumerFactory;
    private readonly IConsumerHandlerFactory<TKey, TValue> _consumerHandlerFactory;
    private readonly IKafkaHandlerFactory<TKey, TValue> _kafkaHandlerFactory;
    private readonly ILoggerFactory _loggerFactory;
    
    private readonly ILogger _logger;

    public ConsumerTaskFactory(
        RetryableConsumerConfig retryableConsumerConfig,
        IHandler<TKey, TValue> payloadHandler,
        // IKafkaConsumerFactory<TKey, TValue> kafkaConsumerFactory, 
        IConsumerHandlerFactory<TKey, TValue> consumerHandlerFactory,
        IKafkaHandlerFactory<TKey, TValue> kafkaHandlerFactory,
        ILoggerFactory loggerFactory)
    {
        _retryableConsumerConfig = retryableConsumerConfig;
        _payloadHandler = payloadHandler;
        // _kafkaConsumerFactory = kafkaConsumerFactory;
        _consumerHandlerFactory = consumerHandlerFactory;
        _kafkaHandlerFactory = kafkaHandlerFactory;
        _loggerFactory = loggerFactory;
        _logger = loggerFactory.CreateLogger<ConsumerTaskFactory<TKey, TValue>>();
    }

    public List<IConsumerTask> CreateTaskConsumers()
    {
        var consumers = new List<IConsumerTask>();
        var topicConfigs = FlattenTopicConfigs();

        foreach (var topicConfig in topicConfigs)
        {
            consumers.Add(CreateTaskConsumer(topicConfig));
        }
        
        return consumers;
    }

    private List<GenericTopicConfig> FlattenTopicConfigs()
    {
        var configs = new List<GenericTopicConfig>();

        var groupId = _retryableConsumerConfig.GroupId;
        var server = _retryableConsumerConfig.Server;
        
        // Add Main topic config
        configs.Add(new GenericTopicConfig(
            server,
            _retryableConsumerConfig.Topic,
            groupId
            )
        );

        foreach (var retryConfig in _retryableConsumerConfig.Retries)
        {
            // Add Retry topic config
            configs.Add(new GenericTopicConfig(
                retryConfig.Server?? server,
                retryConfig.Topic,
                retryConfig.GroupId?? groupId,
                retryConfig.Delay,
                retryConfig.Attempts
                )
            );
        }
        
        if (_retryableConsumerConfig.Dlq is not null && _retryableConsumerConfig.Dlq.Topic is not null)
        {
            // Add Dlq topic config
            configs.Add(new GenericTopicConfig(
                server,
                _retryableConsumerConfig.Dlq.Topic)
            );
        }
        
        return configs;
    }
    
    private IConsumerTask CreateTaskConsumer(GenericTopicConfig config)
    {
        
    }
    
    // public List<IConsumerTask> CreateTaskConsumers()
    // {
    //     var consumers = new List<IConsumerTask>();
    //
    //     consumers.Add(CreateMainTaskConsumer());
    //     
    //     return consumers;
    // }
    //
    // private IConsumerTask CreateMainTaskConsumer()
    // {
    //     var kafkaConsumer = _kafkaConsumerFactory.CreateKafkaConsumer(new ConsumerConfig()
    //     {
    //         BootstrapServers = _retryableConsumerConfig.Server,
    //         GroupId = _retryableConsumerConfig.GroupId,
    //     });
    //     
    //     var kafkaHandler = _kafkaHandlerFactory.CreateMainHandler(_payloadHandler, _retryableConsumerConfig);
    //
    //     return new ConsumerTask<TKey, TValue>(
    //         _retryableConsumerConfig.Topic,
    //         kafkaConsumer,
    //         kafkaHandler,
    //         _loggerFactory);
    // }
    //
    // private IConsumerTask CreateRetryTaskConsumer(
    //     RetryableConsumerConfig retryableConsumerConfig, 
    //     IHandler<TKey, TValue> payloadHandler)
    // {
    //     throw new NotImplementedException();
    // }
}