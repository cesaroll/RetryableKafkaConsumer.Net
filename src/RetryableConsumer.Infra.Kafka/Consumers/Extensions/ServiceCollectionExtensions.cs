using Microsoft.Extensions.DependencyInjection;
using RetryableConsumer.Domain.Configs;
using RetryableConsumer.Infra.Kafka.Consumers.Config;
using RetryableConsumer.Infra.Kafka.Consumers.Factories;
using RetryableConsumer.Infra.Kafka.Consumers.Strategy;

namespace RetryableConsumer.Infra.Kafka.Consumers.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection RegisterConsumerServices<TKey, TValue>(
        this IServiceCollection services,
        RegistrationConfig config
    )
    {
        services.AddSingleton<IConsumerWrapperFactory<TKey, TValue>, ConsumerWrapperFactory<TKey, TValue>>();
        
        var consumerWrapperFactory = services.BuildServiceProvider().GetRequiredService<IConsumerWrapperFactory<TKey, TValue>>();
        
        services.AddSingleton(
            consumerWrapperFactory.Create(new ConsumerWrapperConfig(
                config.Id,
                config.Main.Host,
                config.Main.Topic,
                config.Main.GroupId
            )));

        foreach (var retry in config.Retries)
        {
            services.AddSingleton(
                consumerWrapperFactory.Create(new ConsumerWrapperConfig(
                    config.Id,
                    retry.Host,
                    retry.Topic,
                    retry.GroupId,
                    retry.Delay,
                    retry.Attempts
                )));
        }

        services.AddSingleton<IConsumerStrategy<TKey, TValue>, ConsumerStrategy<TKey, TValue>>();
        
        return services;
    }
}