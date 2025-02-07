using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using RetryableConsumer.Abstractions.Handlers;
using RetryableConsumer.Channels.Extensions;
using RetryableConsumer.Mapper;
using RetryableConsumer.Serializers;
using Config = RetryableConsumer.Abstractions.Configs.Config;

namespace RetryableConsumer.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection RegisterRetryableConsumer<TKey, TValue, THandler>(
        this IServiceCollection services,
        Config config)
        where THandler : IHandler<TKey, TValue>
    {
        services.Configure<HostOptions>(opt =>
        {
            opt.ServicesStartConcurrently = true;
            opt.ServicesStopConcurrently = true;
        });
        
        services.AddSerializers<TKey, TValue>();

        var registrationConfig = config.ToRegistrationConfig();

        services.RegisterChannels<TKey, TValue>(registrationConfig);
        
        return services;
    }
    
    private static IServiceCollection AddSerializers<TKey, TValue>(this IServiceCollection services)
    {
        services.TryAddSingleton<ISerializer<TKey>>(new JsonSerializer<TKey>());
        services.TryAddSingleton<ISerializer<TValue>>(new JsonSerializer<TValue>());
        services.TryAddSingleton<IDeserializer<TKey>>(new JsonSerializer<TKey>());
        services.TryAddSingleton<IDeserializer<TValue>>(new JsonSerializer<TValue>());
        return services;
    }
}