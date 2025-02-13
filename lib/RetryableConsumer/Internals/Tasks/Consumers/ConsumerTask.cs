using System.Text.Json;
using System.Threading.Channels;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using RetryableConsumer.Internals.Channels;
using RetryableConsumer.Internals.Channels.Extensions;

namespace RetryableConsumer.Internals.Tasks.Consumers;

internal class ConsumerTask<TKey, TValue> : ITask
{
    private readonly string _topic;
    private readonly IConsumer<TKey, TValue> _consumer;
    private readonly ChannelWriter<ChannelRequest<TKey, TValue>> _outChannelWriter;
    private readonly ChannelReader<ChannelRequest<TKey, TValue>> _inCommitChannelReader;
    private readonly ILogger<ConsumerTask<TKey, TValue>> _logger;
    private const int ContinuosErrorLimit = 3;
    private int _continuousErrorCount = 0;
    
    public ConsumerTask(
        string topic,
        IConsumer<TKey, TValue> consumer,
        ChannelWriter<ChannelRequest<TKey, TValue>> outChannelWriter,
        ChannelReader<ChannelRequest<TKey, TValue>> inCommitChannelReader,
        ILogger<ConsumerTask<TKey, TValue>> logger)
    {
        _topic = topic;
        _consumer = consumer;
        _outChannelWriter = outChannelWriter;
        _inCommitChannelReader = inCommitChannelReader;
        _logger = logger;
    }
    
    public async Task Run(CancellationToken ct)
    {
        ConsumerSubscribe();
        
        var consumerTask = ConsumeAsync(ct);
        var commitTask = CommitAsync(ct);

        await Task.Run(() => Task.WhenAll(consumerTask, commitTask), ct);
    }
    
    private void ConsumerSubscribe()
    {
        _logger.LogInformation($"Subscribing to topic: {_topic}");
        _consumer.Subscribe(_topic);
        _logger.LogInformation($"Subscribed to topic: {_topic}");
    }
    
    private async Task ConsumeAsync(CancellationToken ct)
    {
        _logger.LogInformation($"Consuming messages from topic: {_topic}");

        while (!ct.IsCancellationRequested)
        {
            try
            {
                var consumeResult = await Task.Run(() => _consumer.Consume(ct), ct);
                _logger.LogDebug(
                    $"Consumed message from topic: {_topic}. " +
                    $"Message: {JsonSerializer.Serialize(consumeResult.Message.Value)}");
                
                await WriteToOutChannelAsync(consumeResult, ct);
                _continuousErrorCount = 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                await HandleConsumerError();
            }
        }
        
        _consumer.Close();
        _logger.LogInformation($"Stopped message consumption from topic: {_topic}");
    }
    
    private async Task CommitAsync(CancellationToken ct)
    {
        _logger.LogInformation($"Committing messages topic: {_topic}");

        while (!ct.IsCancellationRequested)
        {
            try
            {
                var request = await _inCommitChannelReader.ReadAsync(ct);
                var consumeResult = request.ConsumeResult;
                
                _logger.LogDebug(
                    $"Committing message topic: {_topic}. " +
                    $"Message: {JsonSerializer.Serialize(consumeResult.Message.Value)}");
                
                await Task.Run(() => _consumer.Commit(consumeResult), ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
        }
    }
    
    private async Task WriteToOutChannelAsync(
        ConsumeResult<TKey, TValue> consumeResult,
        CancellationToken ct)
    {
        try
        {
            await _outChannelWriter.WriteWithTimeOutAsync(new ChannelRequest<TKey, TValue>(consumeResult), ct);
        } catch (OperationCanceledException ex)
        {
            var msg = $"Writing to main channel timed out.";
            throw new TimeoutException(msg, ex);
        }
    }

    private async Task HandleConsumerError()
    {
        _continuousErrorCount++;
        if (_continuousErrorCount > ContinuosErrorLimit)
            await ReBalanceConsumer(5);
        else
            await PauseConsumerAsync(5);
    }
    
    private async Task PauseConsumerAsync(int seconds)
    {
        _logger.LogInformation($"Pausing consumer for {seconds} seconds.");
        _consumer.Pause(_consumer.Assignment);
        await Task.Delay(TimeSpan.FromSeconds(seconds));
        _consumer.Resume(_consumer.Assignment);
        _logger.LogInformation($"Resumed consumer.");
    }

    private async Task ReBalanceConsumer(int seconds) // TODO: Allow this to happen from an endpoint
    {
        _logger.LogInformation("Rebalancing consumer.");
        _consumer.Close();
        await Task.Delay(TimeSpan.FromSeconds(seconds));
        ConsumerSubscribe();
        _logger.LogInformation($"Rebalanced consumer.");
    }
}