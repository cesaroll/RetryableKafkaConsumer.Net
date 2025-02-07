using Confluent.Kafka;

namespace RetryableConsumer.Internals.Channels;

internal record ChannelRequest<TKey, TValue>(
    ConsumeResult<TKey, TValue> ConsumeResult);