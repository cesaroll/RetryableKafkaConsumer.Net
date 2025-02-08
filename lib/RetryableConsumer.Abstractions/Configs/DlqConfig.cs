namespace RetryableConsumer.Abstractions.Configs;

public class DlqConfig
{
    public string? Topic { get; init; }
    public string? Host { get; set; }
    public required int ConcurrencyDegree { get; init; } = 1;
    public required int ChannelCapacity { get; init; } = 10;
}