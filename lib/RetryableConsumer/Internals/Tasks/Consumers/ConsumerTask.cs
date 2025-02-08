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
                
                await WriteToOutChannelAsync(consumeResult, ct);
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
                var request = await _inCommitChannelReader.ReadAsync(ct);
                var consumeResult = request.ConsumeResult;
                
                _logger.LogDebug(
                    $"Committing message topic: {_topic}. " +
                    $"Message: {JsonSerializer.Serialize(consumeResult.Message.Value)}");
                
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
}