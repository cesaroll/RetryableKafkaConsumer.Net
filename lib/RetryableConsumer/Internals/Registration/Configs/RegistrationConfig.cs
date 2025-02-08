namespace RetryableConsumer.Internals.Registration.Configs;

internal class RegistrationConfig
{
    public required MainConfig Main { get; init; }
    public List<RetryConfig> Retries { get; set; } = [];
    public DlqConfig? Dlq { get; set; }
}