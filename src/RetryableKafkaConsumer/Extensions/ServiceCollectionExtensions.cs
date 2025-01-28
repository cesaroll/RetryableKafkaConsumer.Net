using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RetryableKafkaConsumer.Consumers;
using RetryableKafkaConsumer.Consumers.Kafka;
using RetryableKafkaConsumer.Contracts.Configs;
using RetryableKafkaConsumer.Contracts.Handlers;
using RetryableKafkaConsumer.Handlers;
using RetryableKafkaConsumer.HostedServices;
using RetryableKafkaConsumer.Producers;
using RetryableKafkaConsumer.Serializers;

namespace RetryableKafkaConsumer.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection RegisterRetryableConsumer<TKey, TValue, THandler>(
        this IServiceCollection services,
        RetryableConsumerConfig config)
        where THandler : IHandler<TKey, TValue>
    {
        services.Configure<HostOptions>(opt =>
        {
            opt.ServicesStartConcurrently = true;
            opt.ServicesStopConcurrently = true;
        });
        
        services.AddSingleton<ISerializer<TKey>>(new JsonSerializer<TKey>());
        services.AddSingleton<ISerializer<TValue>>(new JsonSerializer<TValue>());
        
        services.AddSingleton<IProducerFactory<TKey, TValue>, ProducerFactory<TKey, TValue>>();
        
        services.AddSingleton<IKafkaHandlerFactory<TKey, TValue>, KafkaHandlerFactory<TKey, TValue>>();
        
        services.AddSingleton<IKafkaConsumerFactory<TKey, TValue>, KafkaConsumerFactory<TKey,TValue>>();
        
        services.AddSingleton<IConsumerTaskFactory>(provider => 
            ActivatorUtilities.CreateInstance<ConsumerTaskFactory<TKey, TValue>>(
                provider,
                config,
                ActivatorUtilities.CreateInstance<THandler>(services.BuildServiceProvider())
                )
        );
        
        services.AddHostedService<ConsumerHostedService<TKey, TValue>>();

        PrintRegisteredServices(services);
        
        return services;
    }

    private static void PrintRegisteredServices(IServiceCollection services)
    {
        foreach (var service in services)
        {
            if (service.ServiceType.Namespace != null && 
                (service.ServiceType.Namespace.StartsWith("Microsoft") || 
                 service.ServiceType.Namespace.StartsWith("System")))
            continue;
            
            Console.WriteLine($"  {service.ServiceType.Name,-30} -> {service.ImplementationType?.Name, -30} as {service.Lifetime} -> {service.ServiceType.Namespace}");
        }
    }
}