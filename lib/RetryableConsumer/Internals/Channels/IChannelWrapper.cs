using System.Threading.Channels;

namespace RetryableConsumer.Internals.Channels;

internal interface IChannelWrapper<TKey, TValue>
{
    string Id { get;}
    ChannelType ChannelType { get; }
    Channel<ChannelRequest<TKey, TValue>> Channel { get;}
}