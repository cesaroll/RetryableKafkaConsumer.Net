using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using RetryableConsumer.Abstractions.Handlers;
using RetryableConsumer.Infra.Kafka.Consumers.Extensions;
using RetryableConsumer.Infra.Kafka.Producers.Extensions;
using RetryableConsumer.Mapper;
using RetryableConsumer.Serializers;
using Config = RetryableConsumer.Abstractions.Configs.Config;

namespace RetryableConsumer.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection RegisterRetryableConsumer<TKey, TValue, THandler>(
        this IServiceCollection services,
        string registrationId,
        Config config)
        where THandler : IHandler<TKey, TValue>
    {
        services.Configure<HostOptions>(opt =>
        {
            opt.ServicesStartConcurrently = true;
            opt.ServicesStopConcurrently = true;
        });

        services.TryAddSingleton<ISerializer<TKey>>(new JsonSerializer<TKey>());
        services.TryAddSingleton<ISerializer<TValue>>(new JsonSerializer<TValue>());

        var registrationConfig = config.ToRegistrationConfig(registrationId);

        services.RegisterProducerServices<TKey, TValue>(registrationConfig);
        services.RegisterConsumerServices<TKey, TValue>(registrationConfig);

        return services;
    }
}