namespace RetryableKafkaConsumer.Contracts.Configs;

public class RetryConfig
{
    public required string Topic { get; init; }
    public string? GroupId { get; init; }
    public string? Server { get; init; }
    public required TimeSpan Delay { get; init; }
    public int Attempts { get; init; } = 1;
}
