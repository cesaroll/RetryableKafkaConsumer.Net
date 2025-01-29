using Confluent.Kafka;

namespace RetryableConsumer.Abstractions.Handlers;

public interface IValueHandler<TValue> : IHandler<Ignore, TValue>
{
}