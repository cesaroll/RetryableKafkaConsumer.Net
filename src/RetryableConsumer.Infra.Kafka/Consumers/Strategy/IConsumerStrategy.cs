namespace RetryableConsumer.Infra.Kafka.Consumers.Strategy;

public interface IConsumerStrategy<TKey, TValue>
{
    IConsumerWrapper<TKey, TValue>? GetConsumer(string registrationId, string topic);
}