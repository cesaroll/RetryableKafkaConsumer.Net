using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using RetryableKafkaConsumer.Contracts.Results;

namespace RetryableKafkaConsumer.Producers;

internal class RetryEventProducer<TKey, TValue> : IEventProducer<TKey, TValue>
{
    private readonly IProducer<TKey, TValue> _producer;
    private readonly string _topic;
    private readonly ILogger _logger;

    public RetryEventProducer(
        IProducer<TKey, TValue> producer,
        string topic,
        // ISerializer<TKey> keySerializer,
        // ISerializer<TValue> valueSerializer,
        ILoggerFactory loggerFactory)
    {
        _producer = producer;
        _topic = topic;
        _logger = loggerFactory.CreateLogger<RetryEventProducer<TKey, TValue>>();

        // _topic = "test-topic-retry1";
        // _producer = new ProducerBuilder<TKey, TValue>(new ProducerConfig
        //     {
        //         BootstrapServers = "localhost:9092"
        //     })
        //     .SetKeySerializer(keySerializer)
        //     .SetValueSerializer(valueSerializer)
        //     .Build();
    }


    public async Task<Result> ProduceAsync(ConsumeResult<TKey, TValue> consumeResult, CancellationToken ct)
    {
        var message = new Message<TKey, TValue>
        {
            Key = consumeResult.Message.Key,
            Value = consumeResult.Message.Value,
            Headers = consumeResult.Message.Headers
        };
        
        try
        {
            await _producer.ProduceAsync(_topic, message, ct);
            _logger.LogInformation($"Produced message to topic: {_topic}");
            return new SuccessResult();
            
        } catch(Exception ex)
        {
            _logger.LogError(ex, "An error occurred while producing message");
            throw;
        }
    }
}