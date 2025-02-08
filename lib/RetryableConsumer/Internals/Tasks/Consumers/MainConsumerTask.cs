using System.Text.Json;
using System.Threading.Channels;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using RetryableConsumer.Internals.Channels;

namespace RetryableConsumer.Internals.Tasks.Consumers;

internal class MainConsumerTask<TKey, TValue> : ITask
{
    private static readonly TimeSpan TimeoutDuration = TimeSpan.FromSeconds(3);
    private readonly string _topic;
    private readonly IConsumer<TKey, TValue> _consumer;
    private readonly ChannelWriter<ChannelRequest<TKey, TValue>> _mainChannelWriter;
    private readonly ChannelReader<ChannelRequest<TKey, TValue>> _commitChannelReader;
    private readonly ILogger<MainConsumerTask<TKey, TValue>> _logger;

    public MainConsumerTask(
        string topic,
        IConsumer<TKey, TValue> consumer,
        ChannelWriter<ChannelRequest<TKey, TValue>> mainChannelWriter,
        ChannelReader<ChannelRequest<TKey, TValue>> commitChannelReader,
        ILogger<MainConsumerTask<TKey, TValue>> logger)
    {
        _topic = topic;
        _consumer = consumer;
        _mainChannelWriter = mainChannelWriter;
        _commitChannelReader = commitChannelReader;
        _logger = logger;
    }

    public async Task Run(CancellationToken ct)
    {
        ConsumerSubscribe();
        
        var consumerTask = ConsumerRunAsync(ct);
        var commitTask = CommitRunAsync(ct);
        
        await Task.WhenAll(consumerTask, commitTask);
    }

    private void ConsumerSubscribe()
    {
        _logger.LogInformation($"Subscribing to topic: {_topic}");
        _consumer.Subscribe(_topic);
        _logger.LogInformation($"Subscribed to topic: {_topic}");
    }

    private async Task ConsumerRunAsync(CancellationToken ct)
    {
        _logger.LogInformation($"Consuming messages from topic: {_topic}");

        while (!ct.IsCancellationRequested)
        {
            try
            {
                var consumeResult = await ConsumeAsync(ct);
                _logger.LogDebug(
                    $"Consumed message from topic: {_topic}. " +
                    $"Message: {JsonSerializer.Serialize(consumeResult.Message.Value)}");
                
                await WriteToMainChannelAsync(consumeResult, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
        }
        
        _consumer.Close();
        _logger.LogInformation($"Stopped message consumption from topic: {_topic}");
    }
    
    private async Task CommitRunAsync(CancellationToken ct)
    {
        _logger.LogInformation($"Committing messages topic: {_topic}");

        while (!ct.IsCancellationRequested)
        {
            try
            {
                var request = await _commitChannelReader.ReadAsync(ct);
                _logger.LogDebug(
                    $"Committing message topic: {_topic}. " +
                    $"Message: {JsonSerializer.Serialize(request.ConsumeResult.Message.Value)}");
                var consumeResult = request.ConsumeResult;
                _consumer.Commit(consumeResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
        }
    }
    
    private async Task<ConsumeResult<TKey, TValue>> ConsumeAsync(CancellationToken ct)
        => await Task.Run(() => _consumer.Consume(ct), ct);

    private async Task WriteToMainChannelAsync(
            ConsumeResult<TKey, TValue> consumeResult,
            CancellationToken ct)
    {
        using var cts = new CancellationTokenSource(TimeoutDuration);
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, ct);
        
        try
        {
            await _mainChannelWriter.WriteAsync(new ChannelRequest<TKey, TValue>(consumeResult), linkedCts.Token);
        } catch (OperationCanceledException ex)
        {
            var msg = $"Writing to main channel timed out after {TimeoutDuration.TotalSeconds} seconds.";
            throw new TimeoutException(msg, ex);
        }
    }
}