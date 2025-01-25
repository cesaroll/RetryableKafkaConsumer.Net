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
        _logger.LogInformation("Handling message: {message}", consumeResult.Message.Value);
        throw new Exception("Test exception");
        return Task.FromResult<Result>(new SuccessResult());
    }
}