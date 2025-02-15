using Microsoft.Extensions.DependencyInjection;
using RetryableConsumer.Abstractions.Handlers;
using RetryableConsumer.Internals.Channels.Strategy;
using RetryableConsumer.Internals.Registration.Configs;

namespace RetryableConsumer.Internals.Tasks.Processors.Extensions;

internal static class ServiceCollectionExtensions
{
    internal static IServiceCollection RegisterProcessorTasks<TKey, TValue, THandler>(
        this IServiceCollection services,
        RegistrationConfig config,
        IChannelStrategy<TKey, TValue> channelStrategy
        )
        where THandler : IHandler<TKey, TValue>
    {
        services.RegisterMainProcessorTasks<TKey, TValue, THandler>(config, channelStrategy);
        services.RegisterRetryProcessorTasks<TKey, TValue, THandler>(config, channelStrategy);
        
        return services;
    }
    
    private static void RegisterMainProcessorTasks<TKey, TValue, THandler>(
        this IServiceCollection services,
        RegistrationConfig config,
        IChannelStrategy<TKey, TValue> channelStrategy)
        where THandler : IHandler<TKey, TValue>
    {
        var retryTopic = config.Retries.FirstOrDefault()?.Topic;

        for(var concurrencyIndex=0; concurrencyIndex < config.Main.ConcurrencyDegree; concurrencyIndex++)
        {
            var id = "MainProcessorTask-" + concurrencyIndex;
            
            services.AddSingleton<ITask>(prov =>
                ActivatorUtilities.CreateInstance<MainProcessorTask<TKey, TValue>>(
                    prov,
                    id,
                    ActivatorUtilities.CreateInstance<THandler>(prov),
                    channelStrategy,
                    retryTopic
                    )
            );
        }
        
    }

    private static void RegisterRetryProcessorTasks<TKey, TValue, THandler>(
        this IServiceCollection services,
        RegistrationConfig config,
        IChannelStrategy<TKey, TValue> channelStrategy)
        where THandler : IHandler<TKey, TValue>
    {
        var retries = config.Retries;

        for (var retryConfigIndex = 0; retryConfigIndex < retries.Count; retryConfigIndex++)
        {
            var current = retries[retryConfigIndex];
            var next = (retryConfigIndex < retries.Count - 1) ? retries[retryConfigIndex + 1] : null;

            var maxLocalRetryAttempts = current.Attempts;
            var retryDelay = current.Delay;
            var currentTopic = current.Topic;
            var nextTopic = next?.Topic;

            for (var concurrencyIndex = 0; concurrencyIndex < current.ConcurrencyDegree; concurrencyIndex++)
            {
                var id = $"RetryProcessorTask-{currentTopic}-{concurrencyIndex}";

                services.AddSingleton<ITask>(prov => 
                    ActivatorUtilities.CreateInstance<RetryProcessorTask<TKey, TValue>>(
                        prov,
                        id,
                        ActivatorUtilities.CreateInstance<THandler>(prov),
                        channelStrategy,
                        new RetryProcessorTaskOptions()
                            {
                                MaxLocalRetryAttempts = maxLocalRetryAttempts,
                                RetryDelay = retryDelay,
                                CurrentTopic = currentTopic,
                                NextTopic = nextTopic
                            }
                        )
                    );
            }
        }
    }
}