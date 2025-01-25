namespace RetryableKafkaConsumer.Contracts.Results;

public record RetryResult(string Message, Exception? Exception) : Result(Message, Exception);