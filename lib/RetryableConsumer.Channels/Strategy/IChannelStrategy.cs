namespace RetryableConsumer.Channels;

internal interface IChannelStrategy<TKey, TValue>
{
    IChannelWrapper<TKey, TValue> GetMainChannel();
    IChannelWrapper<TKey, TValue> GetCommitChannel();
    IChannelWrapper<TKey, TValue>? GetRetryChannel(string id);
    IChannelWrapper<TKey, TValue>? GetDlqChannel();
}