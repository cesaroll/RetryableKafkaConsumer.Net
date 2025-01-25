namespace RetryableKafkaConsumer.Contracts.Errors;

public record Error(string? Message, Exception? Exception);