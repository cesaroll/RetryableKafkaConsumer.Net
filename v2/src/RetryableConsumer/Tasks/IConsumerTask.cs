namespace RetryableConsumer.Tasks;

public interface IConsumerTask
{
    public Task Run(CancellationToken ct);
}