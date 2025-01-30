using System.Text.Json;
using Confluent.Kafka;
using RetryableConsumer.Abstractions.Handlers;
using RetryableConsumer.Abstractions.Results;

namespace Harness.Consumers.Handlers;

public class TestHandler: IValueHandler<TestMessage>
{
    private readonly ILogger<TestHandler> _logger;
    
    public TestHandler(ILogger<TestHandler> logger)
    {
        _logger = logger;
    }

    public Task<Result> HandleAsync(ConsumeResult<Ignore, TestMessage> consumeResult, CancellationToken ct)
    {
        var value = consumeResult.Message.Value!;
        
        var json = JsonSerializer.Serialize(consumeResult.Message.Value);
        
        _logger.LogInformation($"Handling message: {json} in topic: {consumeResult.TopicPartitionOffset.Topic}");

        if (value.Value.Contains("retry"))
            return Task.FromResult<Result>(new RetryResult());
        
        if (value.Value.Contains("dlq"))
            return Task.FromResult<Result>(new DlqResult());
        
        if (value.Value.Contains("exception"))
            throw new Exception("A test exception from payload handler");
        
        return Task.FromResult<Result>(new SuccessResult());
    }
}