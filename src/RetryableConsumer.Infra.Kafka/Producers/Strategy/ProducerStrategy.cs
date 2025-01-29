namespace RetryableConsumer.Infra.Kafka.Producers.Strategy;

public class ProducerStrategy<TKey, TValue> : IProducerStrategy<TKey, TValue>
{
    private readonly IEnumerable<IProducerWrapper<TKey, TValue>> _producers;

    public ProducerStrategy(IEnumerable<IProducerWrapper<TKey, TValue>> producers)
    {
        _producers = producers;
    }

    public IProducerWrapper<TKey, TValue>? GetProducer(string registrationId, string topic)
        => _producers
            .Where(x => x.RegistrationId == registrationId)
            .FirstOrDefault(x => x.Topic == topic);
}