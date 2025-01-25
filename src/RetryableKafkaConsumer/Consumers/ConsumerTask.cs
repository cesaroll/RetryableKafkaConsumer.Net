using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using RetryableKafkaConsumer.Contracts.Handlers;
using RetryableKafkaConsumer.Contracts.Results;

namespace RetryableKafkaConsumer.Consumers;

internal class ConsumerTask<TKey, TValue> : IConsumerTask
{
    private readonly string _topic;
    private readonly IConsumer<TKey, TValue> _kafkaConsumer;
    private readonly IHandler<TKey, TValue> _payloadHandler;
    private readonly ILogger _logger;

    public ConsumerTask(
        string topic, 
        IConsumer<TKey, TValue> kafkaConsumer, 
        IHandler<TKey, TValue> payloadHandler, 
        ILoggerFactory loggerFactory)
    {
        _topic = topic;
        _kafkaConsumer = kafkaConsumer;
        _payloadHandler = payloadHandler;
        _logger = loggerFactory.CreateLogger<ConsumerTask<TKey, TValue>>();
    }

    public async Task Run(CancellationToken ct)
    {
        Subscribe();
        await ConsumeAsync(ct);
    }
    
    private void Subscribe()
    {
        _logger.LogInformation($"Subscribing to topic: {_topic}");
        _kafkaConsumer.Subscribe(_topic);
        _logger.LogInformation($"Subscribed to topic: {_topic}");
    }

    private async Task ConsumeAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                var consumeResult = _kafkaConsumer.Consume(ct);
                var result = await _payloadHandler.HandleAsync(consumeResult, ct);

                if (result is SuccessResult or RetryResult or DlqResult)
                {
                    _kafkaConsumer.Commit();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while consuming messages");
            }
        }
        
        _kafkaConsumer.Close();
    }
}