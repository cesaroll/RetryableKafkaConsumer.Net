using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using RetryableConsumer.Internals.Channels.Strategy;
using RetryableConsumer.Internals.Registration.Configs;
using RetryableConsumer.Internals.Tasks.Consumers;

namespace RetryableConsumer.Internals.Tasks.Extensions;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection RegisterTasks<TKey, TValue>(
        this IServiceCollection services, 
        RegistrationConfig config)
    {
        var serviceProvider = services.BuildServiceProvider();
        var channelStrategy = serviceProvider.GetRequiredService<IChannelStrategy<TKey, TValue>>();
        var keyDeserializer = serviceProvider.GetRequiredService<IDeserializer<TKey>>();
        var valueDeserializer = serviceProvider.GetRequiredService<IDeserializer<TValue>>();
        
        services.RegisterMainConsumerTask(
            config.Main, channelStrategy, keyDeserializer, valueDeserializer);
        
        return services;
    }
    
    private static void RegisterMainConsumerTask<TKey, TValue>(
        this IServiceCollection services,
        MainConfig config,
        IChannelStrategy<TKey, TValue> channelStrategy,
        IDeserializer<TKey> keyDeserializer,
        IDeserializer<TValue> valueDeserializer)
    {
        var topic = config.Topic;
        var mainChannelWriter = channelStrategy.GetMainChannel().Channel.Writer;
        var commitChannelReader = channelStrategy.GetCommitChannel().Channel.Reader;
        var consumer = CreateConsumer(config.Host, config.GroupId, keyDeserializer, valueDeserializer);

        services.AddSingleton<ITask>(prov =>
            ActivatorUtilities.CreateInstance<MainConsumerTask<TKey, TValue>>(
                prov,
                topic,
                consumer,
                mainChannelWriter,
                commitChannelReader)
            );
    }
    
    private static IConsumer<TKey, TValue> CreateConsumer<TKey, TValue>(
        string host,
        string groupId,
        IDeserializer<TKey> keyDeserializer,
        IDeserializer<TValue> valueDeserializer)
    {
        var consumerBuilder = new ConsumerBuilder<TKey, TValue>(new ConsumerConfig()
            {
                BootstrapServers = host,
                GroupId = groupId,
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false
            })
            .SetValueDeserializer(valueDeserializer);
        
        if (keyDeserializer is not IDeserializer<Ignore>)
            consumerBuilder.SetKeyDeserializer(keyDeserializer);
        
        return consumerBuilder.Build();
    }
}