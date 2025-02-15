using System.Text.Json;
using Confluent.Kafka;
using Harness.Models;
using RetryableConsumer.Abstractions.Handlers;
using RetryableConsumer.Abstractions.Results;

namespace Harness.Consumers.Handlers;

public class TestHandler: IValueHandler<TestMessage>
{
    private readonly ILogger<TestHandler> _logger;
    private readonly Random _random;
    
    public TestHandler(ILogger<TestHandler> logger)
    {
        _logger = logger;
        _random = new Random();
    }

    public async Task<Result> HandleAsync(ConsumeResult<Ignore, TestMessage> consumeResult, CancellationToken ct)
    {
        var value = consumeResult.Message.Value!;
        
        var json = JsonSerializer.Serialize(consumeResult.Message.Value);
        
        if (_random.Next(1, 100) == 1)
        {
            _logger.LogInformation($"Handling message: {json} in topic: {consumeResult.TopicPartitionOffset.Topic}");    
        }

        await Task.Delay(50, ct);

        if (value.Value.Contains("retry"))
            return RetryResult.Instance;

        if (value.Value.Contains("dlq"))
            return DlqResult.Instance;
        
        if (value.Value.Contains("exception"))
            throw new Exception("A test exception from payload handler");

        return SuccessResult.Instance;
    }
}