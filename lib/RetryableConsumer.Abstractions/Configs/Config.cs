namespace RetryableConsumer.Abstractions.Configs;

public class Config
{
    public required string Topic { get; init; }
    public required string GroupId { get; init; }
    public required string Host { get; set; }
    public required int InfraRetries { get; init; } = 3;
    public required int ConcurrencyDegree { get; init; } = 1;
    public required int ChannelCapacity { get; init; } = 2;
    public List<RetryConfig> Retries { get; init; } = [];
    public DlqConfig? Dlq { get; init; }
}