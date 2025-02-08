using RetryableConsumer.Abstractions.Configs;
using RetryableConsumer.Internals.Registration.Configs;
using DlqConfig = RetryableConsumer.Internals.Registration.Configs.DlqConfig;
using RetryConfig = RetryableConsumer.Internals.Registration.Configs.RetryConfig;

namespace RetryableConsumer.Internals.Registration.Mappers;

internal static class RegistrationConfigsMapper
{
    public static RegistrationConfig ToRegistrationConfig(this Config configurations)
    {
        var config = new RegistrationConfig()
        {
            Main = new MainConfig()
            {
                Topic = configurations.Topic,
                Host = configurations.Host,
                GroupId = configurations.GroupId,
                ConcurrencyDegree = configurations.ConcurrencyDegree,
                ChannelCapacity = configurations.ChannelCapacity
            }
        };

        foreach (var retry in configurations.Retries)
        {
            config.Retries.Add(new RetryConfig()
            {
                Topic = retry.Topic,
                Host = retry.Host?? configurations.Host,
                GroupId = retry.GroupId?? configurations.GroupId + "_retry",
                Delay = retry.Delay,
                Attempts = retry.Attempts,
                InfraRetries = configurations.InfraRetries,
                ConcurrencyDegree = retry.ConcurrencyDegree,
                ChannelCapacity = retry.ChannelCapacity
            });
        }
        
        if (configurations.Dlq != null && configurations.Dlq.Topic != null)
        {
            config.Dlq = new DlqConfig()
            {
                Topic = configurations.Dlq.Topic,
                Host = configurations.Dlq.Host?? configurations.Host,
                InfraRetries = configurations.InfraRetries,
                ConcurrencyDegree = configurations.Dlq.ConcurrencyDegree,
                ChannelCapacity = configurations.Dlq.ChannelCapacity
            };
        }
        
        return config;
    }
}