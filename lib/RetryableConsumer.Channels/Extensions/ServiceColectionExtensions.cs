using System.Threading.Channels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RetryableConsumer.Domain.Configs;

namespace RetryableConsumer.Channels.Extensions;

public static class ServiceColectionExtensions
{
    public static IServiceCollection RegisterChannels<TKey, TValue>(
        this IServiceCollection services, RegistrationConfig config)
    {
        var channelCapacity = config.ProcessorCount * 2;
        
        // Setup Main Channel
        services.AddSingleton<IChannelWrapper<TKey, TValue>>(_ =>
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
        
        // Setup Commit Channel
        services.AddSingleton<IChannelWrapper<TKey, TValue>>(_ =>
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
        
        // Setup Retry Channels (if any)
        foreach (var retry in config.Retries)
        {
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
            );
        }
        
        // Setup Dlq Channel
        services.AddSingleton<IChannelWrapper<TKey, TValue>>(_ =>
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
        
        return services;
    }
}