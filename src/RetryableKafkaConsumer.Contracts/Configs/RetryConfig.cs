namespace RetryableKafkaConsumer.Contracts.Configs;

public record RetryConfig(
    TimeSpan Delay,
    int Attempts
);