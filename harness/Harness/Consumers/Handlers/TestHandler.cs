using Confluent.Kafka;
using RetryableKafkaConsumer.Contracts.Handlers;
using RetryableKafkaConsumer.Contracts.Results;

namespace Harness.Consumers.Handlers;

public class TestHandler: IValueHandler<string>
{
    private readonly ILogger<TestHandler> _logger;
    
    public TestHandler(ILogger<TestHandler> logger)
    {
        Name = "Handler1";
        _logger = logger;
    }

    public string Name { get; }

    public Task<Result> HandleAsync(ConsumeResult<Ignore, string> consumeResult, CancellationToken ct)
    {
        _logger.LogInformation("Handling message: {message}", consumeResult.Message.Value);
        return Task.FromResult<Result>(new SuccessResult());
    }
}