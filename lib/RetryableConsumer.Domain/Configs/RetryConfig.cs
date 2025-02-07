namespace RetryableConsumer.Domain.Configs;

public class RetryConfig
{
    public required string Topic { get; set; }
    public required string Host { get; set; }
    public required string GroupId { get; set; }
    public required TimeSpan Delay { get; set; }
    public required int Attempts { get; set; }
    public required int InfraRetries { get; set; }
}