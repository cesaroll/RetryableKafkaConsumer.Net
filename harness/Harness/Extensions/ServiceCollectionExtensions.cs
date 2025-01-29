using Confluent.Kafka;
using Harness.Consumers.Handlers;
using RetryableConsumer.Extensions;
using Config = RetryableConsumer.Abstractions.Configs.Config;

namespace Harness.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection RegisterConsumers(
        this IServiceCollection services,
        IConfigurationManager configManager)
    {
        var config = configManager
            .GetRequiredSection("RetryableKafkaConsumer")
            .Get<Config>()!;

        services.RegisterRetryableConsumer<Ignore, string, TestHandler>("Replica-1", config);
            
        return services;
    }
}