using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RetryableConsumer.Abstractions.Handlers;
using RetryableConsumer.Domain.Configs;
using RetryableConsumer.Infra.Kafka.Consumers;
using RetryableConsumer.Infra.Kafka.Consumers.Strategy;
using RetryableConsumer.Infra.Kafka.Producers;
using RetryableConsumer.Infra.Kafka.Producers.Strategy;
using RetryableConsumer.Processor.Config;
using RetryableConsumer.Processor.Processors;

namespace RetryableConsumer.Processor.Factories;

public class ProcessorFactory<TKey, TValue, THandler> : 
    IProcessorFactory
    where THandler : IHandler<TKey, TValue>
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IProducerStrategy<TKey, TValue> _producerStrategy;
    private readonly IConsumerStrategy<TKey, TValue> _consumerStrategy;
    private readonly ILoggerFactory _loggerFactory;
    
    public ProcessorFactory(
        IServiceProvider serviceProvider,
        IProducerStrategy<TKey, TValue> producerStrategy,
        IConsumerStrategy<TKey, TValue> consumerStrategy,
        ILoggerFactory loggerFactory
    )
    {
        _serviceProvider = serviceProvider;
        _producerStrategy = producerStrategy;
        _consumerStrategy = consumerStrategy;
        _loggerFactory = loggerFactory;
    }
    
    public IList<IProcessor> CreateProcessors(RegistrationConfig config)
    {
        var processors = new List<IProcessor>();
        
        processors.Add(
            CreateMainTopicProcessor(
                new ProcessorConfig(
                    config.Id,
                    config.Main.Topic,
                    config.Retries.FirstOrDefault()?.Topic,
                    config.Dlq?.Topic),
                ActivatorUtilities.CreateInstance<THandler>(_serviceProvider) 
            )
        );
        
        var retries = config.Retries;
        for (var i = 0; i < retries.Count; i++)
        {
            var current = retries[i];
            var next = (i < retries.Count - 1) ? retries[i + 1] : null;
            
            processors.Add(
                CreateRetryTopicProcessor(new ProcessorConfig(
                        config.Id,
                        current.Topic,
                        next?.Topic,
                        config.Dlq?.Topic),
                    ActivatorUtilities.CreateInstance<THandler>(_serviceProvider) 
                )
            );
        }

        return processors;
    }

    private IProcessor CreateMainTopicProcessor(ProcessorConfig config, IHandler<TKey, TValue> payloadHandler)
        => new MainTopicProcessor<TKey, TValue>(
            GetConsumer(config.RegistrationId, config.ConsumerTopic)!,
            payloadHandler,
            GetProducer(config.RegistrationId, config.RetryTopic),
            GetProducer(config.RegistrationId, config.DlqTopic),
            _loggerFactory.CreateLogger<MainTopicProcessor<TKey, TValue>>());

    private IProcessor CreateRetryTopicProcessor(ProcessorConfig config, IHandler<TKey, TValue> payloadHandler)
        => new RetryTopicProcessor<TKey, TValue>(
            GetConsumer(config.RegistrationId, config.ConsumerTopic)!,
            payloadHandler,
            GetProducer(config.RegistrationId, config.ConsumerTopic),
            GetProducer(config.RegistrationId, config.RetryTopic),
            GetProducer(config.RegistrationId, config.DlqTopic),
            _loggerFactory.CreateLogger<RetryTopicProcessor<TKey, TValue>>());
    
    private IConsumerWrapper<TKey, TValue>? GetConsumer(string registrationId, string? topic)
        => topic is null ? null : _consumerStrategy.GetConsumer(registrationId, topic);
    
    private IProducerWrapper<TKey, TValue>? GetProducer(string registrationId, string? topic)
        => topic is null ? null : _producerStrategy.GetProducer(registrationId, topic);
    
}