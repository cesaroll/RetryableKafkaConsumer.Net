namespace RetryableConsumer.Internals.Registration.Configs;

internal class DlqConfig
{
    public required string Topic { get; init; }
    public required string Host { get; init; }
    public required int InfraRetries { get; init; }
    public required int ConcurrencyDegree { get; init; }
    public required int ChannelCapacity { get; init; }
}