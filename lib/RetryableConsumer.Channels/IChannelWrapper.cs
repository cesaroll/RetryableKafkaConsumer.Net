using System.Threading.Channels;

namespace RetryableConsumer.Channels;

internal interface IChannelWrapper<TKey, TValue>
{
    string Id { get;}
    ChannelType ChannelType { get; }
    Channel<ChannelRequest<TKey, TValue>> Channel { get;}
}