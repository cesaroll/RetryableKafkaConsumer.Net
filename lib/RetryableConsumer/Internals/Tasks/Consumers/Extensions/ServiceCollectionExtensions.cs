using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using RetryableConsumer.Internals.Channels.Strategy;
using RetryableConsumer.Internals.Registration.Configs;

namespace RetryableConsumer.Internals.Tasks.Consumers.Extensions;

internal static class ServiceCollectionExtensions
{
    internal static void RegisterMainConsumerTask<TKey, TValue>(
        this IServiceCollection services,
        MainConfig config,
        IChannelStrategy<TKey, TValue> channelStrategy,
        IDeserializer<TKey> keyDeserializer,
        IDeserializer<TValue> valueDeserializer)
    {
        var topic = config.Topic;
        var mainChannelWriter = channelStrategy.GetMainChannel().Channel.Writer;
        var commitChannelReader = channelStrategy.GetCommitChannel().Channel.Reader;
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