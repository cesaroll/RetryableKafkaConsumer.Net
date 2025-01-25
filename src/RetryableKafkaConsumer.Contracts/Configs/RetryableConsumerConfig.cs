namespace RetryableKafkaConsumer.Contracts.Configs;

public record RetryableConsumerConfig(
    string Topic,
    string GroupId,
    string Server,
    List<RetryConfig> Retries
    // DlqConfig Dlq
);