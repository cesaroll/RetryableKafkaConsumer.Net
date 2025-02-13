using System.Text.Json;
using System.Threading.Channels;
using Microsoft.Extensions.Logging;
using RetryableConsumer.Abstractions.Handlers;
using RetryableConsumer.Abstractions.Results;
using RetryableConsumer.Internals.Channels;
using RetryableConsumer.Internals.Channels.Extensions;

namespace RetryableConsumer.Internals.Tasks.Processors;

internal abstract class BaseProcessorTask<TKey, TValue> : ITask
{
    protected readonly string Id;
    protected readonly IHandler<TKey, TValue> PayloadHandler;
    protected readonly ChannelReader<ChannelRequest<TKey, TValue>> InRequestChannelReader;
    protected readonly ChannelWriter<ChannelRequest<TKey, TValue>> OutCommitChannelWriter;
    protected readonly ChannelWriter<ChannelRequest<TKey, TValue>>? OutRetryChannelWriter;
    protected readonly ChannelWriter<ChannelRequest<TKey, TValue>>? OutDlqChannelWriter;
    protected readonly ILogger<BaseProcessorTask<TKey, TValue>> Logger;

    protected BaseProcessorTask(
        string id,
        IHandler<TKey, TValue> payloadHandler,
        ChannelReader<ChannelRequest<TKey, TValue>> inRequestChannelReader, 
        ChannelWriter<ChannelRequest<TKey, TValue>> outCommitChannelWriter, 
        ChannelWriter<ChannelRequest<TKey, TValue>>? outRetryChannelWriter, 
        ChannelWriter<ChannelRequest<TKey, TValue>>? outDlqChannelWriter, 
        ILogger<BaseProcessorTask<TKey, TValue>> logger)
    {
        Id = id;
        PayloadHandler = payloadHandler;
        InRequestChannelReader = inRequestChannelReader;
        OutCommitChannelWriter = outCommitChannelWriter;
        OutRetryChannelWriter = outRetryChannelWriter;
        OutDlqChannelWriter = outDlqChannelWriter;
        Logger = logger;
    }

    public async Task Run(CancellationToken ct)
    {
        await ProcessAsync(ct);
    }

    private async Task ProcessAsync(CancellationToken ct)
    {
        Logger.LogInformation("{Id} started.", Id);

        while (!ct.IsCancellationRequested)
        {
            try
            {
                var channelRequest = await ReadFromInChannel(ct);
                if (channelRequest == null)
                    continue;
                
                var result = await TryHandleAsync(channelRequest, ct);
                
                if (result is not ErrorResult)
                {
                    await WriteToOutCommitChannelAsync(channelRequest, ct);
                }
                
            } catch(Exception ex)
            {
                Logger.LogError(ex, ex.Message);
            }
        }
    }

    private async Task<ChannelRequest<TKey, TValue>?> ReadFromInChannel(CancellationToken ct)
    {
        try
        {
            var request = await InRequestChannelReader.ReadAsync(ct);
            var message = request.ConsumeResult.Message;
            Logger.LogDebug(
                "Processor id: {Id}. " +
                "Received request from channel." +
                "Message: {JsonSerializer.Serialize(message.Value)}", 
                Id, JsonSerializer.Serialize(message.Value));
            return request;
        } catch(Exception ex)
        {
            Logger.LogError(ex, $"Error reading from channel: {Id}");
        }

        return null;
    }
    
    private async Task<Result> TryHandleAsync(ChannelRequest<TKey, TValue> channelRequest, CancellationToken ct)
    {
        try
        {
            var result = await PayloadHandler.HandleAsync(channelRequest.ConsumeResult, ct);

            return result switch
            {
                SuccessResult => result,
                RetryResult => await TryRetry(channelRequest, ct),
                DlqResult => await TryDlq(channelRequest, ct),
                _ => result
            };
        }
        catch (Exception ex)
        {
            var msg = "An error occurred while handling messages in processor";
            Logger.LogError(ex, "Processor id: {Id}. " + msg, Id);

            return await TryDlq(channelRequest, ct);
        }
    }
    
    protected abstract Task<Result> TryRetry(ChannelRequest<TKey, TValue> channelRequest, CancellationToken ct);
    
    protected async Task<Result> TryDlq(ChannelRequest<TKey, TValue> channelRequest, CancellationToken ct)
    {
        if (OutDlqChannelWriter == null)
            return ErrorResult.Instance;

        try
        {
            await OutDlqChannelWriter.WriteWithTimeOutAsync(channelRequest, ct);
        } 
        catch (OperationCanceledException ex)
        {
            Logger.LogError(ex, "Processor id: {Id}. Writing to DLQ channel timed out.", Id);
            return ErrorResult.Instance;
        }

        return SuccessResult.Instance;
    }
    
    private async Task WriteToOutCommitChannelAsync(ChannelRequest<TKey, TValue> channelRequest, CancellationToken ct)
    {
        try
        {
            await OutCommitChannelWriter.WriteWithTimeOutAsync(channelRequest, ct);
        } 
        catch (OperationCanceledException ex)
        {
            Logger.LogError(ex, "Processor id: {Id}. Writing to commit channel timed out.", Id);
        }
    }
}