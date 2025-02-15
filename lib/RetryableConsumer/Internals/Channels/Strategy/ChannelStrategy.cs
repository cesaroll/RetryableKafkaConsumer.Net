namespace RetryableConsumer.Internals.Channels.Strategy;

internal class ChannelStrategy<TKey, TValue> : IChannelStrategy<TKey, TValue>
{
    private readonly IEnumerable<IChannelWrapper<TKey, TValue>> _channels;

    public ChannelStrategy(IEnumerable<IChannelWrapper<TKey, TValue>> channels)
    {
        _channels = channels;
    }


    public IChannelWrapper<TKey, TValue> GetMainConsumerChannel()
        => _channels
            .Where(x => x.ChannelType == ChannelType.MainConsumer)
            .FirstOrDefault(x => x.Id == ChannelType.MainConsumer.ToString())!;

    public IChannelWrapper<TKey, TValue> GetMainCommitChannel()
        => _channels
            .Where(x => x.ChannelType == ChannelType.MainConsumerCommit)
            .FirstOrDefault(x => x.Id == ChannelType.MainConsumerCommit.ToString())!;

    public IChannelWrapper<TKey, TValue>? GetRetryProducerChannel(string id)
        => _channels
            .Where(x => x.ChannelType == ChannelType.RetryProducer)
            .FirstOrDefault(x => x.Id == id);
    
    public IChannelWrapper<TKey, TValue>? GetDlqProducerChannel()
        => _channels
            .Where(x => x.ChannelType == ChannelType.DlqProducer)
            .FirstOrDefault(x => x.Id == ChannelType.DlqProducer.ToString());
    
    public IChannelWrapper<TKey, TValue>? GetRetryConsumerChannel(string id)
        => _channels
            .Where(x => x.ChannelType == ChannelType.RetryConsumer)
            .FirstOrDefault(x => x.Id == id);
    
    public IChannelWrapper<TKey, TValue>? GetRetryConsumerCommitChannel(string id)
        => _channels
            .Where(x => x.ChannelType == ChannelType.RetryConsumerCommit)
            .FirstOrDefault(x => x.Id == id);
}