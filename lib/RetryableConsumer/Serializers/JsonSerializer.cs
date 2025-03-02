using System.Text.Json;
using Confluent.Kafka;

namespace RetryableConsumer.Serializers;

public class JsonSerializer<T> : ISerializer<T>, IDeserializer<T>
{
    public byte[] Serialize(T data, SerializationContext context)
     => JsonSerializer.SerializeToUtf8Bytes(data, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

    public T Deserialize(ReadOnlySpan<byte> data, bool isNull, SerializationContext context)
        => JsonSerializer.Deserialize<T>(data, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
}