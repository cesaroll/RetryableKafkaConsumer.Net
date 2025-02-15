using System.Text.Json;
using System.Threading.Channels;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using RetryableConsumer.Internals.Channels;

namespace RetryableConsumer.Internals.Tasks.Producers;

internal class ProducerTask<TKey, TValue> : ITask
{
    private readonly string _topic;
    private readonly ChannelReader<ChannelRequest<TKey, TValue>> _inChannelReader;
    private readonly IProducer<TKey, TValue> _producer;
    private readonly ILogger<ProducerTask<TKey, TValue>> _logger;

    public ProducerTask(
        string topic, 
        ChannelReader<ChannelRequest<TKey, TValue>> inChannelReader, 
        IProducer<TKey, TValue> producer,
        ILogger<ProducerTask<TKey, TValue>> logger)
    {
        _topic = topic;
        _inChannelReader = inChannelReader;
        _producer = producer;
        _logger = logger;
    }

    public async Task Run(CancellationToken ct)
    {
        await ProcessAsync(ct);
    }
    
    private async Task ProcessAsync(CancellationToken ct)
    {
        _logger.LogInformation($"Producer started for topic: {_topic}");

        while (!ct.IsCancellationRequested)
        {
            try
            {
                var channelRequest = await ReadFromInChannel(ct);
                if (channelRequest == null)
                    continue;
                
                await ProduceAsync(channelRequest.ConsumeResult.Message, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
        }
    }
    
    private async Task<ChannelRequest<TKey, TValue>?> ReadFromInChannel(CancellationToken ct)
    {
        try
        {
            var request = await _inChannelReader.ReadAsync(ct);
            var message = request.ConsumeResult.Message;
            _logger.LogDebug(
                "Producer for topic: {_topic}. " +
                "Received request from channel." + 
                "Message: {JsonSerializer.Serialize(message.Value)}", 
                _topic,
                JsonSerializer.Serialize(message.Value));
            return request;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error reading from channel: {_topic}");
        }

        return null;
    }

    private async Task ProduceAsync(Message<TKey, TValue> message, CancellationToken ct)
    {
        try
        {
            await _producer.ProduceAsync(_topic, message, ct);
            _logger.LogDebug("Produced message to topic: {_topic}", _topic);
        } catch (Exception ex)
        {
            var msg = $"An error occurred while producing message to topic: {_topic}";
            throw new Exception(msg, ex);
        }
    }
}