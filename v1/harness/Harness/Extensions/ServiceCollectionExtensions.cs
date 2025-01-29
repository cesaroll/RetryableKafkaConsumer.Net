using Confluent.Kafka;
using Harness.Consumers.Handlers;
using RetryableKafkaConsumer.Contracts.Configs;
using RetryableKafkaConsumer.Extensions;

namespace Harness.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection RegisterConsumers(
        this IServiceCollection services,
        IConfigurationManager configManager)
    {
        var config = configManager
            .GetRequiredSection("RetryableKafkaConsumer")
            .Get<RetryableConsumerConfig>()!;

        services.RegisterRetryableConsumer<Ignore, string, TestHandler>(config);
            
        return services;
    }
}