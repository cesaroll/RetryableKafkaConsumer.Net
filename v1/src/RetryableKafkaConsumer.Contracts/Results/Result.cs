namespace RetryableKafkaConsumer.Contracts.Results;

public abstract record Result(string? Message = null, Exception? Exception = null);