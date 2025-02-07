namespace RetryableConsumer.Channels;

internal class ChannelStrategy<TKey, TValue> : IChannelStrategy<TKey, TValue>
{
    private readonly IEnumerable<IChannelWrapper<TKey, TValue>> _channels;

    public ChannelStrategy(IEnumerable<IChannelWrapper<TKey, TValue>> channels)
    {
        _channels = channels;
    }


    public IChannelWrapper<TKey, TValue> GetMainChannel()
        => _channels
            .Where(x => x.ChannelType == ChannelType.Main)
            .FirstOrDefault(x => x.Id == ChannelType.Main.ToString())!;

    public IChannelWrapper<TKey, TValue> GetCommitChannel()
        => _channels
            .Where(x => x.ChannelType == ChannelType.Commit)
            .FirstOrDefault(x => x.Id == ChannelType.Commit.ToString())!;

    public IChannelWrapper<TKey, TValue>? GetRetryChannel(string id)
        => _channels
            .Where(x => x.ChannelType == ChannelType.Retry)
            .FirstOrDefault(x => x.Id == id);
    
    public IChannelWrapper<TKey, TValue>? GetDlqChannel()
        => _channels
            .Where(x => x.ChannelType == ChannelType.Dlq)
            .FirstOrDefault(x => x.Id == ChannelType.Dlq.ToString());
}