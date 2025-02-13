using System.Threading.Channels;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using RetryableConsumer.Abstractions.Handlers;
using RetryableConsumer.Abstractions.Results;
using RetryableConsumer.Internals.Channels;
using RetryableConsumer.Internals.Channels.Extensions;
using RetryableConsumer.Internals.Tasks.Processors.Extensions;

namespace RetryableConsumer.Internals.Tasks.Processors;

internal class MainProcessorTask<TKey, TValue> : BaseProcessorTask<TKey, TValue>
{
    public MainProcessorTask(
        string id, 
        IHandler<TKey, TValue> payloadHandler, 
        ChannelReader<ChannelRequest<TKey, TValue>> inRequestChannelReader, 
        ChannelWriter<ChannelRequest<TKey, TValue>> outCommitChannelWriter, 
        ChannelWriter<ChannelRequest<TKey, TValue>>? outRetryChannelWriter, 
        ChannelWriter<ChannelRequest<TKey, TValue>>? outDlqChannelWriter, 
        ILogger<MainProcessorTask<TKey, TValue>> logger) : 
        base(
            id, 
            payloadHandler, 
            inRequestChannelReader, 
            outCommitChannelWriter, 
            outRetryChannelWriter, 
            outDlqChannelWriter, 
            logger)
    {
    }
    
    protected override async Task<Result> TryRetry(
        ChannelRequest<TKey, TValue> channelRequest, CancellationToken ct)
    {
        var currentMessage = channelRequest.ConsumeResult.Message;

        var newMessage = new Message<TKey, TValue>
        {
            Key = currentMessage.Key,
            Value = currentMessage.Value,
            Headers = currentMessage.Headers
        };
        
        newMessage.SetLocalRetryCountHeader(1);
        newMessage.SetOverallRetryCountHeader(1);
        
        channelRequest.ConsumeResult.Message = newMessage;

        if (OutRetryChannelWriter != null)
        {
            try
            {
                await OutRetryChannelWriter.WriteWithTimeOutAsync(channelRequest, ct);
                return SuccessResult.Instance;
            } catch (OperationCanceledException ex)
            {
                Logger.LogError(ex, "Processor id: {Id}. Writing to Retry channel timed out.", Id);
                return ErrorResult.Instance;
            }
        }
        
        return await TryDlq(channelRequest, ct);
    }
}