namespace RetryableKafkaConsumer.Contracts.Configs;

public class RetryableConsumerConfig
{
    public required string Topic { get; init; }
    public required string GroupId { get; init; }
    public required string Server { get; init; }
    public List<RetryConfig> Retries { get; init; } = new();

    // public DlqConfig Dlq { get; init; }
}