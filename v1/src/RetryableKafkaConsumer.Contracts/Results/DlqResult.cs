namespace RetryableKafkaConsumer.Contracts.Results;

public record DlqResult(string? Message = null, Exception? Exception = null) : Result(Message, Exception);