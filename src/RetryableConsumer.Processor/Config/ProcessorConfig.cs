namespace RetryableConsumer.Processor.Config;

public record ProcessorConfig(
    string RegistrationId,
    string ConsumerTopic,
    string? RetryTopic,
    string? DlqTopic
);