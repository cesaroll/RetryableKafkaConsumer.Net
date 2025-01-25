using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RetryableKafkaConsumer.Consumers;
using RetryableKafkaConsumer.Contracts.Configs;
using RetryableKafkaConsumer.Contracts.Handlers;
using RetryableKafkaConsumer.Handlers;
using RetryableKafkaConsumer.HostedServices;
using RetryableKafkaConsumer.Producers;

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
        
        services.AddTransient<IHandler<TKey, TValue>>(provider => 
            ActivatorUtilities.CreateInstance<THandler>(provider)
        );
        
        services.AddSingleton<IKafkaHandlerFactory<TKey, TValue>, KafkaHandlerFactory<TKey, TValue>>();
        services.AddSingleton<IKafkaConsumerFactory<TKey, TValue>, KafkaConsumerFactory<TKey,TValue>>();
        
        services.AddSingleton<IConsumerTaskFactory<TKey, TValue>>(provider => 
            ActivatorUtilities.CreateInstance<ConsumerTaskFactory<TKey, TValue>>(
                provider,
                config
                //provider.GetRequiredService<IHandler<TKey, TValue>>()
                )
        );
        
        services.AddSingleton<IEventProducer, RetryEventProducer>(); // TODO: use factories instead
        
        services.AddHostedService<ConsumerHostedService<TKey, TValue>>();

        return services;
    }
    
    public static IServiceCollection RegisterTestHostedService(this IServiceCollection services)
    {
        services.Configure<HostOptions>(opt =>
        {
            opt.ServicesStartConcurrently = true;
            opt.ServicesStopConcurrently = true;
        });
        
        services.AddHostedService<TestHostedService>();
        
        return services;
    }
}