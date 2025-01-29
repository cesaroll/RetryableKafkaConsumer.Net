using RetryableConsumer.Abstractions.Handlers;

namespace RetryableConsumer.Processor.Processors;

public interface IProcessor
{
    void Subscribe();
    Task ConsumeAsync(CancellationToken ct);
}