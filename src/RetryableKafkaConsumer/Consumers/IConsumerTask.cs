namespace RetryableKafkaConsumer.Consumers;

internal interface IConsumerTask
{
    public Task Run(CancellationToken ct);
}