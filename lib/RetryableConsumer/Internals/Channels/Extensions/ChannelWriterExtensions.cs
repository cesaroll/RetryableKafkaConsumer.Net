using System.Threading.Channels;

namespace RetryableConsumer.Internals.Channels.Extensions;

internal static class ChannelWriterExtensions
{
    private static readonly TimeSpan WriterTimeout = TimeSpan.FromSeconds(5); // TODO: Move to configuration
    
    public static async ValueTask WriteWithTimeOutAsync<TKey, TValue>(
        this ChannelWriter<ChannelRequest<TKey, TValue>> channelWriter,
        ChannelRequest<TKey, TValue> channelRequest,
        CancellationToken ct)
    {
        using var cts = new CancellationTokenSource(WriterTimeout);
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, ct);
        
        await channelWriter.WriteAsync(channelRequest, linkedCts.Token);
    }
}