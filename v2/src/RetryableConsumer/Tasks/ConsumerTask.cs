using RetryableConsumer.Processor.Processors;

namespace RetryableConsumer.Tasks;

public class ConsumerTask : IConsumerTask
{
    private readonly IProcessor _processor;

    public ConsumerTask(IProcessor processor)
    {
        _processor = processor;
    }

    public async Task Run(CancellationToken ct)
    {
        _processor.Subscribe();
        await _processor.ConsumeAsync(ct);
    }
}