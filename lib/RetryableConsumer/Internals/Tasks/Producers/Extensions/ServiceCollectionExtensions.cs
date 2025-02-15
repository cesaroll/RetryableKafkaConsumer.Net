using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using RetryableConsumer.Internals.Channels.Strategy;
using RetryableConsumer.Internals.Registration.Configs;

namespace RetryableConsumer.Internals.Tasks.Producers.Extensions;

internal static class ServiceCollectionExtensions
{
    internal static IServiceCollection RegisterProducerTasks<TKey, TValue>(
        this IServiceCollection services,
        ServiceProvider serviceProvider,
        RegistrationConfig config,
        IChannelStrategy<TKey, TValue> channelStrategy)
    {
        services.RegisterRetryProducerTasks(config.Retries, channelStrategy);
        
        if (config.Dlq != null)
            services.RegisterDlqProducerTask(config.Dlq, channelStrategy);

        return services;
    }

    private static void RegisterRetryProducerTasks<TKey, TValue>(
        this IServiceCollection services,
        List<RetryConfig> retryConfigs,
        IChannelStrategy<TKey, TValue> channelStrategy)
        => retryConfigs.ForEach(config =>
            services.AddSingleton<ITask>(prov =>
                ActivatorUtilities.CreateInstance<ProducerTask<TKey, TValue>>(
                    prov,
                    config.Topic,
                    channelStrategy.GetRetryProducerChannel(config.Topic)!.Channel.Reader,
                    CreateKafkaProducer(
                        config.Host,
                        config.InfraRetries,
                        prov.GetRequiredService<ISerializer<TKey>>(),
                        prov.GetRequiredService<ISerializer<TValue>>()
                    )
                )
            )
        );

    private static void RegisterDlqProducerTask<TKey, TValue>(
        this IServiceCollection services,
        DlqConfig config,
        IChannelStrategy<TKey, TValue> channelStrategy)
        => services.AddSingleton<ITask>(prov =>
            ActivatorUtilities.CreateInstance<ProducerTask<TKey, TValue>>(
                prov,
                config.Topic,
                channelStrategy.GetDlqProducerChannel()!.Channel.Reader,
                CreateKafkaProducer(
                    config.Host,
                    config.InfraRetries,
                    prov.GetRequiredService<ISerializer<TKey>>(),
                    prov.GetRequiredService<ISerializer<TValue>>()
                )
            )
        );
    
    private static IProducer<TKey, TValue> CreateKafkaProducer<TKey, TValue>(
        string host,
        int retries,
        ISerializer<TKey> keySerializer,
        ISerializer<TValue> valueSerializer)
        =>  new ProducerBuilder<TKey, TValue>(new ProducerConfig()
            {
                BootstrapServers = host,
                MessageSendMaxRetries = retries
            })
            .SetKeySerializer(keySerializer)
            .SetValueSerializer(valueSerializer)
            .Build();
}