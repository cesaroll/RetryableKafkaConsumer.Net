using Microsoft.Extensions.Hosting;
using RetryableConsumer.Internals.Tasks;

namespace RetryableConsumer.Internals.Services;

internal class ConsumerHostedService : IHostedService
{
    private readonly IEnumerable<ITask> _tasks;

    public ConsumerHostedService(IEnumerable<ITask> tasks)
    {
        _tasks = tasks;
    }
    
    public async Task StartAsync(CancellationToken ct)
    {
        await Parallel.ForEachAsync(_tasks, ct, async (task, ct) 
            => await task.Run(ct));
    }

    public Task StopAsync(CancellationToken ct)
        => Task.CompletedTask;
}