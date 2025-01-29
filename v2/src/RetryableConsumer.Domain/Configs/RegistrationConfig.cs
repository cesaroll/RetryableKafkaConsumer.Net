namespace RetryableConsumer.Domain.Configs;

public class RegistrationConfig
{
    public required string Id { get; init; }
    public required MainConfig Main { get; init; }
    public List<RetryConfig> Retries { get; set; } = [];
    public DlqConfig? Dlq { get; set; }
}

public class MainConfig
{
    public required string Topic { get; init; }
    public required string Host { get; init; }
    public required string GroupId { get; init; }
}

public class RetryConfig
{
    public required string Topic { get; set; }
    public required string Host { get; set; }
    public required string GroupId { get; set; }
    public required TimeSpan Delay { get; set; }
    public required int Attempts { get; set; }
    public required int InfraRetries { get; set; }
}

public class DlqConfig
{
    public required string Topic { get; init; }
    public required string Host { get; init; }
    public required int InfraRetries { get; init; }
}