using Microsoft.Extensions.Logging;
using RetryableConsumer.Abstractions.Handlers;
using RetryableConsumer.Abstractions.Results;
using RetryableConsumer.Internals.Channels;
using RetryableConsumer.Internals.Channels.Strategy;
using RetryableConsumer.Internals.Tasks.Processors.Extensions;

namespace RetryableConsumer.Internals.Tasks.Processors;

internal class MainProcessorTask<TKey, TValue> : BaseProcessorTask<TKey, TValue>
{
    public MainProcessorTask(
        string id, 
        IHandler<TKey, TValue> payloadHandler, 
        IChannelStrategy<TKey, TValue> channelStrategy,
        string? retryTopic,
        ILogger<MainProcessorTask<TKey, TValue>> logger) : 
        base(
            id, 
            payloadHandler, 
            channelStrategy.GetMainConsumerChannel().Channel.Reader, 
            channelStrategy.GetMainCommitChannel().Channel.Writer, 
            retryTopic != null? channelStrategy.GetRetryProducerChannel(retryTopic)?.Channel.Writer : null, 
            channelStrategy.GetDlqProducerChannel()?.Channel.Writer, 
            logger)
    {
    }

    protected override Task BeforeHandleAsync(ChannelRequest<TKey, TValue> channelRequest, CancellationToken ct)
        => Task.CompletedTask;
    
    protected override async Task<Result> TryRetry(
        ChannelRequest<TKey, TValue> channelRequest, CancellationToken ct)
    {
        var newMessage = CreateNewMessage(channelRequest.ConsumeResult.Message);
        
        newMessage.SetLocalRetryCountHeader(1);
        newMessage.SetOverallRetryCountHeader(1);
        
        channelRequest.ConsumeResult.Message = newMessage;
    
        if (RetryChannelWriter == null) 
            return await TryDlq(channelRequest, ct);
        
        try
        {
            await RetryChannelWriter.WriteAsync(channelRequest, ct);
            return SuccessResult.Instance;
        } catch (OperationCanceledException ex)
        {
            Logger.LogError(ex, "Processor id: {Id}. Writing to Retry channel timed out.", Id);
            return ErrorResult.Instance;
        }
    }
}