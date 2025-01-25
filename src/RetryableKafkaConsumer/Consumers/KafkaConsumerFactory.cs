using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using RetryableKafkaConsumer.Contracts.Configs;
using RetryableKafkaConsumer.Mappers;

namespace RetryableKafkaConsumer.Consumers;

internal class KafkaConsumerFactory<TKey, TValue> : IKafkaConsumerFactory<TKey, TValue>
{
    private readonly ILogger<KafkaConsumerFactory<TKey, TValue>> _logger;
    private readonly ILoggerFactory _loggerFactory;

    public KafkaConsumerFactory(
        ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<KafkaConsumerFactory<TKey, TValue>>();
        _loggerFactory = loggerFactory;
    }

    public IConsumer<TKey, TValue> CreateKafkaConsumer(RetryableConsumerConfig retryableConsumerConfig)
    {
        var builder = new ConsumerBuilder<TKey, TValue>(retryableConsumerConfig.ToConsumerConfig());
        // TODO: Log handlers and others
        return builder.Build();
    }
}