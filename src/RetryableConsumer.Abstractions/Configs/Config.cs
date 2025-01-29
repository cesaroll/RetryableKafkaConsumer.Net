namespace RetryableConsumer.Abstractions.Configs;

public class Config
{
    public required string Topic { get; init; }
    public required string GroupId { get; init; }
    public required string Host { get; init; }
    public required int InfraRetries { get; init; } = 1;
    public List<RetryConfig> Retries { get; init; } = [];
    public DlqConfig? Dlq { get; init; }
}