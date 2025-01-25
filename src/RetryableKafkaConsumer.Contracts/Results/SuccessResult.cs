namespace RetryableKafkaConsumer.Contracts.Results;

public record SuccessResult(string? Message = null) : Result(Message, null);