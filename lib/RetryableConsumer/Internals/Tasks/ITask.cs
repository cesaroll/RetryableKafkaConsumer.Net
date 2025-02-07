namespace RetryableConsumer.Internals.Tasks;

internal interface ITask
{
    public Task Run(CancellationToken ct);
}