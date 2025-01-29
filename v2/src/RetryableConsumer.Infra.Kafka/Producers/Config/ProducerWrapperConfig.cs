namespace RetryableConsumer.Infra.Kafka.Producers.Config;

public record ProducerWrapperConfig(
    string RegistrationId,
    string Host,
    string Topic,
    int InfraRetries);