using System.Text.Json;
using System.Threading.Channels;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using RetryableConsumer.Abstractions.Handlers;
using RetryableConsumer.Abstractions.Results;
using RetryableConsumer.Internals.Channels;

namespace RetryableConsumer.Internals.Tasks.Processors;

internal abstract class BaseProcessorTask<TKey, TValue> : ITask
{
    protected readonly string Id;
    protected readonly IHandler<TKey, TValue> PayloadHandler;
    protected readonly ChannelReader<ChannelRequest<TKey, TValue>> ConsumerChannelReader;
    protected readonly ChannelWriter<ChannelRequest<TKey, TValue>> ConsumerCommitChannelWriter;
    protected readonly ChannelWriter<ChannelRequest<TKey, TValue>>? RetryChannelWriter;
    protected readonly ChannelWriter<ChannelRequest<TKey, TValue>>? DlqChannelWriter;
    protected readonly ILogger<BaseProcessorTask<TKey, TValue>> Logger;

    protected BaseProcessorTask(
        string id,
        IHandler<TKey, TValue> payloadHandler,
        ChannelReader<ChannelRequest<TKey, TValue>> consumerChannelReader, 
        ChannelWriter<ChannelRequest<TKey, TValue>> consumerCommitChannelWriter, 
        ChannelWriter<ChannelRequest<TKey, TValue>>? retryChannelWriter, 
        ChannelWriter<ChannelRequest<TKey, TValue>>? dlqChannelWriter, 
        ILogger<BaseProcessorTask<TKey, TValue>> logger)
    {
        Id = id;
        PayloadHandler = payloadHandler;
        ConsumerChannelReader = consumerChannelReader;
        ConsumerCommitChannelWriter = consumerCommitChannelWriter;
        RetryChannelWriter = retryChannelWriter;
        DlqChannelWriter = dlqChannelWriter;
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
                
                await BeforeHandleAsync(channelRequest, ct);
                
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
            var request = await ConsumerChannelReader.ReadAsync(ct);
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

    protected abstract Task BeforeHandleAsync(ChannelRequest<TKey, TValue> channelRequest, CancellationToken ct);

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
        if (DlqChannelWriter == null)
            return ErrorResult.Instance;

        channelRequest.ConsumeResult.Message = CreateNewMessage(channelRequest.ConsumeResult.Message);

        try
        {
            await DlqChannelWriter.WriteAsync(channelRequest, ct);
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
            await ConsumerCommitChannelWriter.WriteAsync(channelRequest, ct);
        } 
        catch (OperationCanceledException ex)
        {
            Logger.LogError(ex, "Processor id: {Id}. Writing to commit channel timed out.", Id);
        }
    }
    
    protected Message<TKey, TValue> CreateNewMessage(Message<TKey, TValue> message)
        => new()
        {
            Key = message.Key,
            Value = message.Value,
            Headers = message.Headers
        };
}