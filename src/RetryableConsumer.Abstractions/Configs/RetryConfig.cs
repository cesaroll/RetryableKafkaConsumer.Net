namespace RetryableConsumer.Abstractions.Configs;

public class RetryConfig
{
    public required string Topic { get; init; }
    public string? GroupId { get; init; }
    public string? Host { get; init; }
    public required TimeSpan Delay { get; init; } = TimeSpan.FromMinutes(1);
    public required int Attempts { get; init; } = 1;
    
}