namespace RetryableConsumer.Internals.Configs;

internal class RegistrationConfig
{
    public required MainConfig Main { get; init; }
    public required int ProcessorCount { get; init; }
    public List<RetryConfig> Retries { get; set; } = [];
    public DlqConfig? Dlq { get; set; }
}