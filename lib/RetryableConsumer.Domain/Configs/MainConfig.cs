namespace RetryableConsumer.Domain.Configs;

public class MainConfig
{
    public required string Topic { get; init; }
    public required string Host { get; init; }
    public required string GroupId { get; init; }
}