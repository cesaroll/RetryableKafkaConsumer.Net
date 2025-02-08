using Confluent.Kafka;
using Harness.Consumers.Handlers;
using Harness.Initializers;
using Harness.Models;
using Harness.Producers;
using Harness.Producers.Configs;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RetryableConsumer.Extensions;
using RetryableConsumer.Serializers;
using Config = RetryableConsumer.Abstractions.Configs.Config;

namespace Harness.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection RegisterConsumers(
        this IServiceCollection services,
        IConfigurationManager configManager)
    {
        var config = configManager
            .GetRequiredSection("RetryableKafkaConsumer")
            .Get<Config>()!;

        config = services.SetupAspire(configManager, config);
        
        // Retryable Consumer Lib v1
        // services.RegisterRetryableConsumer<Ignore, TestMessage, TestHandler>("Replica-1", config);
        // services.RegisterRetryableConsumer<Ignore, TestMessage, TestHandler>("Replica-2", config);
        // services.RegisterRetryableConsumer<Ignore, TestMessage, TestHandler>("Replica-3", config);
        
        // Retryable Consumer Lib v2
        services.RegisterRetryableConsumer<Ignore, TestMessage, TestHandler>(config);
        
        
        services.TryAddSingleton<ISerializer<TestMessage>>(new JsonSerializer<TestMessage>());

        services.TryAddSingleton(new ProducerServiceConfig(config.Host, config.Topic));
        services.TryAddSingleton<IKafkaProducerService<TestMessage>, KafkaProducerService<TestMessage>>();
        
        return services;
    }
    
    private static Config SetupAspire(
        this IServiceCollection services,
        IConfigurationManager configManager, 
        Config config)
    {   
        var kafkaInitCompletionSource = new TaskCompletionSource<bool>();
        services.AddSingleton(kafkaInitCompletionSource);
        
        var kafkaHost = configManager.GetConnectionString("aspirekafka");
        
        if (kafkaHost is not null)
        {
            config.Host = kafkaHost;
        
            foreach(var retry in config.Retries.Where(x => x.Host is not null))
                retry.Host = kafkaHost;
            
            if(config.Dlq is not null && config.Dlq.Host is not null)
                config.Dlq.Host = kafkaHost;
            
            services.AddSingleton<IHostedService>(prov => 
                ActivatorUtilities.CreateInstance<KafkaTopicInitializer>(
                    prov, 
                    kafkaHost));
        }
        else
        {
            kafkaInitCompletionSource.SetResult(true);
        }

        return config;
    }
}