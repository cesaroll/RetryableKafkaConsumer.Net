namespace RetryableConsumer.Abstractions.Configs;

public class DlqConfig
{
    public string? Topic { get; init; }
    public string? Host { get; set; }
}