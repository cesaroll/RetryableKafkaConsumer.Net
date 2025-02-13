using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RetryableConsumer.Internals.Tasks;

namespace RetryableConsumer.Internals.Services;

internal class ConsumerHostedService : IHostedService
{
    private readonly IEnumerable<ITask> _tasks;
    private readonly TaskCompletionSource<bool> _kafkaInitcompletionSource;
    private readonly ILogger<ConsumerHostedService> _logger;

    public ConsumerHostedService(
        IEnumerable<ITask> tasks,
        TaskCompletionSource<bool> kafkaInitcompletionSource,
        ILogger<ConsumerHostedService> logger)
    {
        _tasks = tasks;
        _kafkaInitcompletionSource = kafkaInitcompletionSource;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken ct)
    {
        await _kafkaInitcompletionSource.Task;
        
        var runningTasks = _tasks.Select(task => task.Run(ct)).ToList();
        await Task.Run(() => Task.WhenAll(runningTasks), ct);
    }

    public Task StopAsync(CancellationToken ct)
        => Task.CompletedTask;
}