namespace RetryableConsumer.Internals.Channels.Strategy;

internal interface IChannelStrategy<TKey, TValue>
{
    IChannelWrapper<TKey, TValue> GetMainConsumerChannel();
    IChannelWrapper<TKey, TValue> GetMainCommitChannel();
    IChannelWrapper<TKey, TValue>? GetRetryProducerChannel(string id);
    IChannelWrapper<TKey, TValue>? GetDlqProducerChannel();

    IChannelWrapper<TKey, TValue>? GetRetryConsumerChannel(string id);
    IChannelWrapper<TKey, TValue>? GetRetryConsumerCommitChannel(string id);
}