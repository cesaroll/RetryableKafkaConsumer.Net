using Confluent.Kafka;
using RetryableKafkaConsumer.Contracts.Handlers;
using RetryableKafkaConsumer.Contracts.Results;

namespace Harness.Consumers.Handlers;

public class TestHandler: IValueHandler<string>
{
    private readonly ILogger<TestHandler> _logger;
    
    public TestHandler(ILogger<TestHandler> logger)
    {
        _logger = logger;
    }

    public Task<Result> HandleAsync(ConsumeResult<Ignore, string> consumeResult, CancellationToken ct)
    {
        var value = consumeResult.Message.Value;
        
        _logger.LogInformation("Handling message: {message}", value);

        if (value.Contains("retry"))
            return Task.FromResult<Result>(new RetryResult());
        
        if (value.Contains("dlq"))
            return Task.FromResult<Result>(new DlqResult());
        
        if (value.Contains("exception"))
            throw new Exception("A test exception from payload handler");
        
        return Task.FromResult<Result>(new SuccessResult());
    }
}