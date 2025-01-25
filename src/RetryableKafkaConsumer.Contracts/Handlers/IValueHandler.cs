using Confluent.Kafka;

namespace RetryableKafkaConsumer.Contracts.Handlers;

public interface IValueHandler<TValue> : IHandler<Ignore, TValue>
{
}