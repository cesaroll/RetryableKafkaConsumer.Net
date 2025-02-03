using System.Text.Json;
using Confluent.Kafka;
using Harness.Models;
using RetryableConsumer.Serializers;

namespace Tests.Integration.Tests.Fixtures.Kafka;

public static class KafkaFixture
{
    private static string? _host;

    public static async Task<string> GetKafkaBrokerHost(HttpClient httpClient)
    {
        if (_host != null)
            return _host;

        var result = await httpClient.GetAsync("/kafka/host");
        result.EnsureSuccessStatusCode();
        var content = await result.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(content);
        _host = json.RootElement.GetProperty("host").GetString();
        return _host!;
    }

    public static IConsumer<Ignore, TestMessage> CreateConsumer(string host, string groupId)
        => new ConsumerBuilder<Ignore, TestMessage>(new ConsumerConfig()
            {
                BootstrapServers = host,
                GroupId = groupId,
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false
            })
            .SetValueDeserializer(new JsonSerializer<TestMessage>())
            .Build();
}