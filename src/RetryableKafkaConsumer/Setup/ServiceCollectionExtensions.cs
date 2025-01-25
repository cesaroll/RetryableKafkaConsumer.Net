using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RetryableKafkaConsumer.Consumers;
using RetryableKafkaConsumer.Contracts.Configs;
using RetryableKafkaConsumer.Contracts.Handlers;
using RetryableKafkaConsumer.Handlers;
using RetryableKafkaConsumer.HostedServices;
using RetryableKafkaConsumer.Producers;

namespace RetryableKafkaConsumer.Setup;

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
        
        services.AddSingleton<IKafkaHandlerFactory<TKey, TValue>, KafkaHandlerFactory<TKey, TValue>>();
        services.AddSingleton<IKafkaConsumerFactory<TKey, TValue>, KafkaConsumerFactory<TKey,TValue>>();
        // services.AddSingleton<IConsumerTaskFactory<TKey, TValue>, ConsumerTaskFactory<TKey, TValue>>();
        services.AddSingleton<IConsumerTaskFactory<TKey, TValue>>(provider => 
            new ConsumerTaskFactory<TKey, TValue>(
                config,
                provider.GetRequiredService<THandler>(),
                provider.GetRequiredService<IKafkaConsumerFactory<TKey, TValue>>(),
                provider.GetRequiredService<IKafkaHandlerFactory<TKey, TValue>>(),
                provider.GetRequiredService<ILoggerFactory>()
            )
        );
        
        services.AddSingleton<IEventProducer, RetryEventProducer>(); // TODO: use factories instead
        
        //var serviceProvider = services.BuildServiceProvider();
        
        // services.AddSingleton(typeof(THandler), provider =>
        //     ActivatorUtilities.CreateInstance(provider, typeof(THandler), "Handler1"));
        
        //var payloadHandler = serviceProvider.GetRequiredService<THandler>();
        
        // _services.FirstOrDefault(service =>
        //     service.GetType().GetProperty("Name")?.GetValue(service)?.ToString() == name);
        
        // var payloadHandler = services
        //     .FirstOrDefault(s => 
        //         s.GetType().GetProperty("Name")?.GetValue(s)?.ToString() == "Handler1");
        //
        
        //var hostedServiceFactory = serviceProvider.GetRequiredService<IConsumerHostedServiceFactory<TKey, TValue, THandler>>();
        //var hostedService = hostedServiceFactory.CreateConsumerHostedService(config, payloadHandler);
        
        //services.AddHostedService(provider => hostedService);
        
        services.AddHostedService<ConsumerHostedService<TKey, TValue>>();
        
        // services.AddHostedService<ConsumerHostedService<TKey, TValue>>();
        // services.AddHostedService(provider => 
        //     new ConsumerHostedService<TKey, TValue>(
        //     provider.GetRequiredService<IConsumerTaskFactory<TKey, TValue>>()
        //     )
        // );

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