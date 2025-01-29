namespace RetryableConsumer.Infra.Kafka.Consumers.Config;

public record ConsumerWrapperConfig(
    string RegistrationId,
    string Host,
    string Topic,
    string GroupId
);