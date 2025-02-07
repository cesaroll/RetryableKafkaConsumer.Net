namespace RetryableConsumer.Domain.Configs;

public class DlqConfig
{
    public required string Topic { get; init; }
    public required string Host { get; init; }
    public required int InfraRetries { get; init; }
}