using System.Text;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using RetryableConsumer.Abstractions.Handlers;
using RetryableConsumer.Abstractions.Results;
using RetryableConsumer.Infra.Kafka.Consumers;
using RetryableConsumer.Infra.Kafka.Producers;

namespace RetryableConsumer.Processor.Processors;

public abstract class Processor<TKey, TValue> : IProcessor
{
    protected readonly IConsumerWrapper<TKey, TValue> _consumer;
    protected readonly IHandler<TKey, TValue> _payloadHandler;
    protected readonly IProducerWrapper<TKey, TValue>? _retryProducer;
    protected readonly IProducerWrapper<TKey, TValue>? _dlqProducer;
    protected readonly ILogger<Processor<TKey, TValue>> _logger;


    public Processor(
        IConsumerWrapper<TKey, TValue> consumer,
        IHandler<TKey, TValue> payloadHandler,
        IProducerWrapper<TKey, TValue>? retryProducer,
        IProducerWrapper<TKey, TValue>? dlqProducer,
        ILogger<Processor<TKey, TValue>> logger)
    {
        _consumer = consumer;
        _payloadHandler = payloadHandler;
        _retryProducer = retryProducer;
        _dlqProducer = dlqProducer;
        _logger = logger;
    }

    public void Subscribe()
    {
        _logger.LogInformation($"Subscribing to topic: {_consumer.Topic}");
        _consumer.Subscribe();
        _logger.LogInformation($"Subscribed to topic: {_consumer.Topic}");
    }

    public async Task ConsumeAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                var consumeResult = await _consumer.ConsumeAsync(ct);
                await BeforeHandleAsync(consumeResult, ct);
                var result = await TryHandleAsync(consumeResult, ct);

                if (result is not ErrorResult)
                {
                    _consumer.Commit();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while consuming messages");
            }
        }

        _consumer.Close();
    }

    protected abstract Task BeforeHandleAsync(ConsumeResult<TKey, TValue> consumeResult, CancellationToken ct);
    
    private async Task<Result> TryHandleAsync(ConsumeResult<TKey, TValue> consumeResult, CancellationToken ct)
    {
        try
        {
            var result = await _payloadHandler.HandleAsync(consumeResult, ct);

            switch (result)
            {
                case SuccessResult:
                    return result;
                case RetryResult:
                    return await TryRetry(consumeResult.Message, ct);
                case DlqResult:
                    return await TryDlq(consumeResult.Message, ct);
                default:
                    return result;
            }

        }
        catch (Exception ex)
        {
            var msg = "An error occurred while handling messages";
            _logger.LogError(ex, msg);

            return await TryDlq(consumeResult.Message, ct);
        }
    }

    protected abstract Task<Result> TryRetry(Message<TKey, TValue> message, CancellationToken ct);
    protected async Task<Result> TryDlq(Message<TKey, TValue> message, CancellationToken ct)
    {
        if(_dlqProducer != null)
            return await _dlqProducer.ProduceAsync(message, ct);
    
        return new ErrorResult();
    }
}