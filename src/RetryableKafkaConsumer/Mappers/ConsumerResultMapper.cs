using Confluent.Kafka;

namespace RetryableKafkaConsumer.Mappers;

public static class ConsumerResultMapper
{
    public static Message<TKey, TValue> ToMessage<TKey, TValue>(this ConsumeResult<TKey, TValue> consumeResult)
    {
        return new Message<TKey, TValue>
        {
            Key = consumeResult.Message.Key,
            Value = consumeResult.Message.Value,
            Headers = consumeResult.Message.Headers
        };
    }
}