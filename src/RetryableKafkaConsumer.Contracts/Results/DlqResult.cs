namespace RetryableKafkaConsumer.Contracts.Results;

public record DlqResult(string Message, Exception? Exception) : Result(Message, Exception);