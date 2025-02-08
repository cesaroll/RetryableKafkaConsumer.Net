namespace RetryableConsumer.Internals.Registration.Configs;

internal class RetryConfig
{
    public required string Topic { get; set; }
    public required string Host { get; set; }
    public required string GroupId { get; set; }
    public required TimeSpan Delay { get; set; }
    public required int Attempts { get; set; }
    public required int InfraRetries { get; set; }
    public required int ConcurrencyDegree { get; init; }
    public required int ChannelCapacity { get; init; }
}