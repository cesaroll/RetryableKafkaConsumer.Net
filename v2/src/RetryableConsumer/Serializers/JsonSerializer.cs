using System.Text.Json;
using Confluent.Kafka;

namespace RetryableConsumer.Serializers;

public class JsonSerializer<T> : ISerializer<T>
{
    public byte[] Serialize(T data, SerializationContext context)
    {
        return JsonSerializer.SerializeToUtf8Bytes(data);
    }

    public T? Deserialize(byte[] data, bool isNull, SerializationContext context)
    {
        return JsonSerializer.Deserialize<T>(data);
    }
}