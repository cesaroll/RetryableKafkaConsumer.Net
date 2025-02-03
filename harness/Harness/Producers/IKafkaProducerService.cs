namespace Harness.Producers;

public interface IKafkaProducerService<in T>
{
    public Task ProduceAsync(T message);
}