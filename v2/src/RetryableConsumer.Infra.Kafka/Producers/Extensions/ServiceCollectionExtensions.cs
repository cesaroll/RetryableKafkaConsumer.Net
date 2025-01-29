using Microsoft.Extensions.DependencyInjection;
using RetryableConsumer.Domain.Configs;
using RetryableConsumer.Infra.Kafka.Producers.Config;
using RetryableConsumer.Infra.Kafka.Producers.Factories;

namespace RetryableConsumer.Infra.Kafka.Producers.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection RegisterProducerServices<TKey, TValue>(
        this IServiceCollection services,
        RegistrationConfig config
        )
    {
        services.AddSingleton<IProducerWrapperFactory<TKey, TValue>, ProducerWrapperFactory<TKey, TValue>>();
        
        var producerWrapperFactory = services.BuildServiceProvider().GetRequiredService<IProducerWrapperFactory<TKey, TValue>>();
        
        foreach (var retry in config.Retries)
        {
            services.AddSingleton(
                producerWrapperFactory.Create(new ProducerWrapperConfig(
                    config.Id,
                    retry.Host,
                    retry.Topic,
                    retry.InfraRetries
                )));
        }

        if (config.Dlq is not null)
        {
            services.AddSingleton(
                producerWrapperFactory.Create(new ProducerWrapperConfig(
                    config.Id,
                    config.Dlq.Host,
                    config.Dlq.Topic,
                    config.Dlq.InfraRetries
                )));
        }
        
        return services;
    }
}