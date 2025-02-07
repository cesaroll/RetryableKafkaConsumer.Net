using Confluent.Kafka;

namespace RetryableConsumer.Channels;

internal record ChannelRequest<TKey, TValue>(
    ConsumeResult<TKey, TValue> ConsumeResult);