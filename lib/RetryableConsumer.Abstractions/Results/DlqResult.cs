namespace RetryableConsumer.Abstractions.Results;

public record DlqResult(string? Message = null, Exception? Exception = null)
    : Result(Message, Exception)
{
    public static DlqResult Instance { get; } = new();
}