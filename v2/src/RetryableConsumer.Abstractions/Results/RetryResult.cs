namespace RetryableConsumer.Abstractions.Results;

public record RetryResult(string? Message = null, Exception? Exception = null) 
    : Result(Message, Exception);