using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using RetryableConsumer.Abstractions.Results;

namespace RetryableConsumer.Infra.Kafka.Producers;

public class ProducerWrapper<TKey, TValue> : IProducerWrapper<TKey, TValue>
{
    private readonly IProducer<TKey, TValue> _producer;
    private readonly ILogger<ProducerWrapper<TKey, TValue>> _logger;

    public string RegistrationId { get; init; }
    public string Topic { get; init; }
    
    public ProducerWrapper(
        string registrationId,
        string topic,
        IProducer<TKey, TValue> producer, 
        ILogger<ProducerWrapper<TKey, TValue>> logger)
    {
        RegistrationId = registrationId;
        Topic = topic;
        _producer = producer;
        _logger = logger;
    }

    public async Task<Result> ProduceAsync(Message<TKey, TValue> message, CancellationToken ct)
    {
        try
        {
            await _producer.ProduceAsync(Topic, message, ct);
            _logger.LogInformation($"Produced message to topic: {Topic}");
            return new SuccessResult();
            
        } catch(Exception ex)
        {
            _logger.LogError(ex, $"An error occurred while producing message to topic: {Topic}");
            throw;
        }
    }
}