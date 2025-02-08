namespace RetryableConsumer.Internals.Registration.Configs;

internal class MainConfig
{
    public required string Topic { get; init; }
    public required string Host { get; init; }
    public required string GroupId { get; init; }
    public required int ConcurrencyDegree { get; init; }
    public required int ChannelCapacity { get; init; }
}