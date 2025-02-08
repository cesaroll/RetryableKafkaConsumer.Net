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
        var channelCapacity = config.ProcessorCount * 2;
        
        services.RegisterMainChannel<TKey, TValue>(channelCapacity);
        services.RegisterCommitChannel<TKey, TValue>(channelCapacity);
        services.RegistryRetryChannels<TKey, TValue>(config, channelCapacity);
        services.RegisterDlqChannel<TKey, TValue>(channelCapacity);

        services.TryAddSingleton<IChannelStrategy<TKey, TValue>, ChannelStrategy<TKey, TValue>>();
        
        return services;
    }

    private static void RegisterMainChannel<TKey, TValue>(this IServiceCollection services, int channelCapacity)
        => services.AddSingleton<IChannelWrapper<TKey, TValue>>(_ =>
            new ChannelWrapper<TKey, TValue>(
                id: ChannelType.Main.ToString(), 
                channelType: ChannelType.Main,
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
    
    private static void RegisterCommitChannel<TKey, TValue>(this IServiceCollection services, int channelCapacity)
        => services.AddSingleton<IChannelWrapper<TKey, TValue>>(_ =>
            new ChannelWrapper<TKey, TValue>(
                id: ChannelType.Commit.ToString(), 
                channelType: ChannelType.Commit,
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

    private static void RegistryRetryChannels<TKey, TValue>(
        this IServiceCollection services,
        RegistrationConfig config,
        int channelCapacity)
        => config.Retries.ForEach(retry => 
            services.AddSingleton<IChannelWrapper<TKey, TValue>>(_ =>
                new ChannelWrapper<TKey, TValue>(
                    id: retry.Topic, 
                    channelType: ChannelType.Retry,
                    channel: Channel.CreateBounded<ChannelRequest<TKey, TValue>>(
                        new BoundedChannelOptions(channelCapacity)
                        {
                            SingleWriter = false,
                            SingleReader = true,
                            FullMode = BoundedChannelFullMode.Wait,
                            AllowSynchronousContinuations = false
                        })
                )
            )
        );
    
    private static void RegisterDlqChannel<TKey, TValue>(this IServiceCollection services, int channelCapacity)
        => services.AddSingleton<IChannelWrapper<TKey, TValue>>(_ =>
            new ChannelWrapper<TKey, TValue>(
                id: ChannelType.Dlq.ToString(), 
                channelType: ChannelType.Dlq,
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
}