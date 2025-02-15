using System.Threading.Channels;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using RetryableConsumer.Abstractions.Handlers;
using RetryableConsumer.Abstractions.Results;
using RetryableConsumer.Internals.Channels;
using RetryableConsumer.Internals.Channels.Extensions;
using RetryableConsumer.Internals.Channels.Strategy;
using RetryableConsumer.Internals.Tasks.Processors.Extensions;

namespace RetryableConsumer.Internals.Tasks.Processors;

internal class RetryProcessorTaskOptions
{
    public int MaxLocalRetryAttempts { get; init; }
    public TimeSpan RetryDelay { get; init; }
    public string CurrentTopic { get; init; }
    public string? NextTopic { get; init; }
}
internal class RetryProcessorTask<TKey, TValue> : BaseProcessorTask<TKey, TValue>
{
    private readonly int _maxLocalRetryAttempts;
    private readonly TimeSpan _retryDelay;
    private readonly ChannelWriter<ChannelRequest<TKey, TValue>>? _nextRetryChannelWriter;
    
    public RetryProcessorTask(
        string id, 
        IHandler<TKey, TValue> payloadHandler, 
        IChannelStrategy<TKey, TValue> channelStrategy,
        RetryProcessorTaskOptions options,
        ILogger<RetryProcessorTask<TKey, TValue>> logger) 
        : 
        base(
            id, 
            payloadHandler, 
            channelStrategy.GetRetryConsumerChannel(options.CurrentTopic)!.Channel.Reader, 
            channelStrategy.GetRetryConsumerCommitChannel(options.CurrentTopic)!.Channel.Writer,
            channelStrategy.GetRetryProducerChannel(options.CurrentTopic)!.Channel.Writer,
            channelStrategy.GetDlqProducerChannel()?.Channel.Writer, 
            logger)
    {   
        _maxLocalRetryAttempts = options.MaxLocalRetryAttempts;
        _retryDelay = options.RetryDelay;
        _nextRetryChannelWriter = options.NextTopic != null
            ? channelStrategy.GetRetryProducerChannel(options.NextTopic)?.Channel.Writer
            : null;
    }
    
    protected override async Task BeforeHandleAsync(ChannelRequest<TKey, TValue> channelRequest, CancellationToken ct)
        => await DelayAsNeededAsync(channelRequest.ConsumeResult.Message, ct);
    
    protected override async Task<Result> TryRetry(ChannelRequest<TKey, TValue> channelRequest, CancellationToken ct)
    {
        var localRetryCount = channelRequest.ConsumeResult.Message.GetLocalRetryCountHeader();
        var overallRetryCount = channelRequest.ConsumeResult.Message.GetOverallRetryCountHeader();
        
        var newMessage = CreateNewMessage(channelRequest.ConsumeResult.Message);
        
        newMessage.SetLocalRetryCountHeader(localRetryCount + 1);
        newMessage.SetOverallRetryCountHeader(overallRetryCount + 1);
        
        channelRequest.ConsumeResult.Message = newMessage;
    
        var channelWriter = GetChannelWriter();
    
        if (channelWriter == null) 
            return await TryDlq(channelRequest, ct);
        
        
        try
        {
            await channelWriter.WriteWithTimeOutAsync(channelRequest, ct);
            return SuccessResult.Instance;
        } catch (OperationCanceledException ex)
        {
            Logger.LogError(ex, "Processor id: {Id}. Writing to Retry channel timed out.", Id);
            return ErrorResult.Instance;
        }
        
        ChannelWriter<ChannelRequest<TKey, TValue>>? GetChannelWriter()
        {
            if (localRetryCount < _maxLocalRetryAttempts)
                return RetryChannelWriter;
            
            newMessage.SetLocalRetryCountHeader(1);
    
            return _nextRetryChannelWriter;
        }
    }
    
    private async Task DelayAsNeededAsync(Message<TKey, TValue> message, CancellationToken ct)
    {
        var currentDateTime = DateTime.UtcNow;
        var shouldRunDateTime = message.Timestamp.UtcDateTime.Add(_retryDelay);
        
        if (shouldRunDateTime <= currentDateTime) 
            return;
        
        var delay = shouldRunDateTime - currentDateTime;
        
        Logger.LogDebug("Processor id: {Id}. Delaying for {Delay}.", Id, delay);
        
        await Task.Delay(delay, ct);
    }
}