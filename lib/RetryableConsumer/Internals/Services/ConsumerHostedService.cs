using Microsoft.Extensions.Hosting;
using RetryableConsumer.Internals.Tasks;

namespace RetryableConsumer.Internals.Services;

internal class ConsumerHostedService : IHostedService
{
    private readonly IEnumerable<ITask> _tasks;
    private readonly TaskCompletionSource<bool> _kafkaInitcompletionSource;

    public ConsumerHostedService(
        IEnumerable<ITask> tasks,
        TaskCompletionSource<bool> kafkaInitcompletionSource)
    {
        _tasks = tasks;
        _kafkaInitcompletionSource = kafkaInitcompletionSource;
    }
    
    public async Task StartAsync(CancellationToken ct)
    {
        await _kafkaInitcompletionSource.Task;
        await Parallel.ForEachAsync(_tasks, ct, async (task, ct) 
            => await task.Run(ct));
    }

    public Task StopAsync(CancellationToken ct)
        => Task.CompletedTask;
}