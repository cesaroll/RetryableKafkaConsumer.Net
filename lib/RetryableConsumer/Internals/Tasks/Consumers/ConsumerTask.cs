using System.Text.Json;
using System.Threading.Channels;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using RetryableConsumer.Internals.Channels;

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
                
                await _outChannelWriter.WriteAsync(new ChannelRequest<TKey, TValue>(consumeResult), ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
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
}