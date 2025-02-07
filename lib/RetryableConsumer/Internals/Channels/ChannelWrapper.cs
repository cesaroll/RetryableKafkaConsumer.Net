using System.Threading.Channels;

namespace RetryableConsumer.Internals.Channels;

internal class ChannelWrapper<TKey, TValue> : IChannelWrapper<TKey, TValue>
{
    public string Id { get; init; }
    public ChannelType ChannelType { get; init; }
    public Channel<ChannelRequest<TKey, TValue>> Channel { get; init; }
    
    public ChannelWrapper(
        string id, 
        ChannelType channelType, 
        Channel<ChannelRequest<TKey, TValue>> channel)
    {
        Id = id;
        ChannelType = channelType;
        Channel = channel;
    }
}