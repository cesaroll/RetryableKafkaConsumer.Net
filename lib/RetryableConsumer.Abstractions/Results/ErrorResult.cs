namespace RetryableConsumer.Abstractions.Results;

public record ErrorResult(string? Message = null, Exception? Exception = null)
    : Result(Message, Exception)
{
    public static ErrorResult Instance { get; } = new();
}