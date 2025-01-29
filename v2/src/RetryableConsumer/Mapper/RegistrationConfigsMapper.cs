using RetryableConsumer.Abstractions.Configs;
using RetryableConsumer.Domain.Configs;
using DlqConfig = RetryableConsumer.Domain.Configs.DlqConfig;
using RetryConfig = RetryableConsumer.Domain.Configs.RetryConfig;

namespace RetryableConsumer.Mapper;

public static class RegistrationConfigsMapper
{
    public static RegistrationConfig ToRegistrationConfig(this Config configurations, string id)
    {
        var config = new RegistrationConfig()
        {
            Id = id,
            Main = new MainConfig()
            {
                Topic = configurations.Topic,
                Host = configurations.Host,
                GroupId = configurations.GroupId
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
                InfraRetries = configurations.InfraRetries
            });
        }
        
        if (configurations.Dlq != null && configurations.Dlq.Topic != null)
        {
            config.Dlq = new DlqConfig()
            {
                Topic = configurations.Dlq.Topic,
                Host = configurations.Dlq.Host?? configurations.Host,
                InfraRetries = configurations.InfraRetries
            };
        }
        
        
        return config;
    }
}