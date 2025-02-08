using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using RetryableConsumer.Abstractions.Handlers;
using RetryableConsumer.Internals.Channels.Strategy;
using RetryableConsumer.Internals.Registration.Configs;
using RetryableConsumer.Internals.Tasks.Consumers.Extensions;
using RetryableConsumer.Internals.Tasks.Processors.Extensions;

namespace RetryableConsumer.Internals.Tasks.Extensions;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection RegisterTasks<TKey, TValue, THandler>(
        this IServiceCollection services, 
        RegistrationConfig config)
        where THandler : IHandler<TKey, TValue>
    {
        var serviceProvider = services.BuildServiceProvider();
        var channelStrategy = serviceProvider.GetRequiredService<IChannelStrategy<TKey, TValue>>();
        var keyDeserializer = serviceProvider.GetRequiredService<IDeserializer<TKey>>();
        var valueDeserializer = serviceProvider.GetRequiredService<IDeserializer<TValue>>();
        
        services.RegisterMainConsumerTask(config.Main, channelStrategy, keyDeserializer, valueDeserializer);
        services.RegisterMainProcessorTasks<TKey, TValue, THandler>(
            serviceProvider, 
            config, 
            channelStrategy);
        
        return services;
    }
}