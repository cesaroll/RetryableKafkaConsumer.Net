namespace RetryableConsumer.Abstractions.Results;

public record SuccessResult(string? Message = null) : Result(Message, null)
{
    public static SuccessResult Instance { get; } = new();
}