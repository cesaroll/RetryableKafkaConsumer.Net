using System.Threading.Channels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RetryableConsumer.Internals.Channels.Strategy;
using RetryableConsumer.Internals.Registration.Configs;

namespace RetryableConsumer.Internals.Channels.Extensions;

internal static class ServiceColectionExtensions
{
    public static IServiceCollection RegisterChannels<TKey, TValue>(
        this IServiceCollection services, RegistrationConfig config)
    {
        services.RegisterMainConsumerChannel<TKey, TValue>(config.Main.ChannelCapacity);
        services.RegisterMainCommitChannel<TKey, TValue>(config.Main.ChannelCapacity);
        
        services.RegistryRetryProducerChannels<TKey, TValue>(config.Retries);
        services.RegisterDlqProducerChannel<TKey, TValue>(config.Dlq?.ChannelCapacity ?? 1);
        
        services.RegisterRetryConsumerChannels<TKey, TValue>(config.Retries);

        services.TryAddSingleton<IChannelStrategy<TKey, TValue>, ChannelStrategy<TKey, TValue>>();
        
        return services;
    }

    private static void RegisterMainConsumerChannel<TKey, TValue>(this IServiceCollection services, int channelCapacity)
        => services.AddSingleton<IChannelWrapper<TKey, TValue>>(_ =>
            new ChannelWrapper<TKey, TValue>(
                id: ChannelType.MainConsumer.ToString(), 
                channelType: ChannelType.MainConsumer,
                channel: Channel.CreateBounded<ChannelRequest<TKey, TValue>>(
                    new BoundedChannelOptions(channelCapacity)
                    {
                        SingleWriter = true,
                        SingleReader = false,
                        FullMode = BoundedChannelFullMode.Wait,
                        AllowSynchronousContinuations = false
                    })
            )
        );
    
    private static void RegisterMainCommitChannel<TKey, TValue>(this IServiceCollection services, int channelCapacity)
        => services.AddSingleton<IChannelWrapper<TKey, TValue>>(_ =>
            new ChannelWrapper<TKey, TValue>(
                id: ChannelType.MainConsumerCommit.ToString(), 
                channelType: ChannelType.MainConsumerCommit,
                channel: Channel.CreateBounded<ChannelRequest<TKey, TValue>>(
                    new BoundedChannelOptions(channelCapacity)
                    {
                        SingleWriter = false,
                        SingleReader = true,
                        FullMode = BoundedChannelFullMode.Wait,
                        AllowSynchronousContinuations = false
                    })
            )
        );

    private static void RegistryRetryProducerChannels<TKey, TValue>(
        this IServiceCollection services,
        List<RetryConfig> retryConfigs)
        => retryConfigs.ForEach(retry => 
            services.AddSingleton<IChannelWrapper<TKey, TValue>>(_ =>
                new ChannelWrapper<TKey, TValue>(
                    id: retry.Topic, 
                    channelType: ChannelType.RetryProducer,
                    channel: Channel.CreateBounded<ChannelRequest<TKey, TValue>>(
                        new BoundedChannelOptions(retry.ChannelCapacity)
                        {
                            SingleWriter = false,
                            SingleReader = true,
                            FullMode = BoundedChannelFullMode.Wait,
                            AllowSynchronousContinuations = false
                        })
                )
            )
        );
    
    private static void RegisterDlqProducerChannel<TKey, TValue>(this IServiceCollection services, int channelCapacity)
        => services.AddSingleton<IChannelWrapper<TKey, TValue>>(_ =>
            new ChannelWrapper<TKey, TValue>(
                id: ChannelType.DlqProducer.ToString(), 
                channelType: ChannelType.DlqProducer,
                channel: Channel.CreateBounded<ChannelRequest<TKey, TValue>>(
                    new BoundedChannelOptions(channelCapacity)
                    {
                        SingleWriter = false,
                        SingleReader = true,
                        FullMode = BoundedChannelFullMode.Wait,
                        AllowSynchronousContinuations = false
                    })
            )
        );
    
    private static void RegisterRetryConsumerChannels<TKey, TValue>(
        this IServiceCollection services,
        List<RetryConfig> retryConfigs)
    {
        foreach (var config in retryConfigs)
        {
            services.AddSingleton<IChannelWrapper<TKey, TValue>>(_ =>
                new ChannelWrapper<TKey, TValue>(
                    id: config.Topic, 
                    channelType: ChannelType.RetryConsumer,
                    channel: Channel.CreateBounded<ChannelRequest<TKey, TValue>>(
                        new BoundedChannelOptions(config.ChannelCapacity)
                        {
                            SingleWriter = true,
                            SingleReader = false,
                            FullMode = BoundedChannelFullMode.Wait,
                            AllowSynchronousContinuations = false
                        })
                )
            );
            
            services.AddSingleton<IChannelWrapper<TKey, TValue>>(_ =>
                new ChannelWrapper<TKey, TValue>(
                    id: config.Topic,
                    channelType: ChannelType.RetryConsumerCommit,
                    channel: Channel.CreateBounded<ChannelRequest<TKey, TValue>>(
                        new BoundedChannelOptions(config.ChannelCapacity)
                        {
                            SingleWriter = false,
                            SingleReader = true,
                            FullMode = BoundedChannelFullMode.Wait,
                            AllowSynchronousContinuations = false
                        })
                )
            );
        }
    }
}