using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using RetryableConsumer.Internals.Channels.Strategy;
using RetryableConsumer.Internals.Registration.Configs;

namespace RetryableConsumer.Internals.Tasks.Consumers.Extensions;

internal static class ServiceCollectionExtensions
{
    internal static IServiceCollection RegisterConsumerTasks<TKey, TValue>(
        this IServiceCollection services,
        RegistrationConfig config,
        IChannelStrategy<TKey, TValue> channelStrategy,
        IDeserializer<TKey> keyDeserializer,
        IDeserializer<TValue> valueDeserializer)
    {
        services.RegisterMainConsumerTask(
            config.Main,
            channelStrategy,
            keyDeserializer,
            valueDeserializer);
        
        services.RegisterRetryConsumerTasks(
            config.Retries,
            channelStrategy,
            keyDeserializer,
            valueDeserializer);

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
        var mainChannelWriter = channelStrategy.GetMainConsumerChannel().Channel.Writer;
        var commitChannelReader = channelStrategy.GetMainCommitChannel().Channel.Reader;
        var consumer = CreateKafkaConsumer(config.Host, config.GroupId, keyDeserializer, valueDeserializer);

        services.AddSingleton<ITask>(prov =>
            ActivatorUtilities.CreateInstance<ConsumerTask<TKey, TValue>>(
                prov,
                topic,
                consumer,
                mainChannelWriter,
                commitChannelReader)
        );
    }

    private static void RegisterRetryConsumerTasks<TKey, TValue>(
        this IServiceCollection services,
        List<RetryConfig> retryConfigs,
        IChannelStrategy<TKey, TValue> channelStrategy,
        IDeserializer<TKey> keyDeserializer,
        IDeserializer<TValue> valueDeserializer)
    {
        foreach (var config in retryConfigs)
        {
            var topic = config.Topic;
            var channelWriter = channelStrategy.GetRetryConsumerChannel(topic)!.Channel.Writer;
            var channelReader = channelStrategy.GetRetryConsumerCommitChannel(topic)!.Channel.Reader;
            var consumer = CreateKafkaConsumer(config.Host, config.GroupId, keyDeserializer, valueDeserializer);

            services.AddSingleton<ITask>(prov =>
                ActivatorUtilities.CreateInstance<ConsumerTask<TKey, TValue>>(
                    prov,
                    topic,
                    consumer,
                    channelWriter,
                    channelReader)
            );
        }
    }
    
    private static IConsumer<TKey, TValue> CreateKafkaConsumer<TKey, TValue>(
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