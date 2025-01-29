namespace RetryableConsumer.Infra.Kafka.Consumers.Strategy;

public class ConsumerStrategy<TKey, TValue> : IConsumerStrategy<TKey, TValue>
{
    private readonly IEnumerable<IConsumerWrapper<TKey, TValue>> _consumers;

    public ConsumerStrategy(IEnumerable<IConsumerWrapper<TKey, TValue>> consumers)
    {
        _consumers = consumers;
    }

    public IConsumerWrapper<TKey, TValue>? GetConsumer(string registrationId, string topic)
        => _consumers
            .Where(x => x.RegistrationId == registrationId)
            .FirstOrDefault(x => x.Topic == topic);
}