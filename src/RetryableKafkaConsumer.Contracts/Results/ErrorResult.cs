namespace RetryableKafkaConsumer.Contracts.Results;

public record ErrorResult(string Message, Exception? Exception = null) : Result(Message, Exception);