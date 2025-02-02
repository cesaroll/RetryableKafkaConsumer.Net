using Confluent.Kafka;
using Confluent.Kafka.Admin;

namespace Harness.Initializers;

public class KafkaTopicInitializer : IHostedService
{
    private readonly string _host;
    private readonly TaskCompletionSource<bool> _kafkaInitcompletionSource;
    private readonly List<(string, int)> _topicNames =
    [
        ("test-topic", 3),
        ("test-topic-retry1", 1),
        ("test-topic-retry2", 1),
        ("test-topic-dlq", 1)
    ];

    public KafkaTopicInitializer(string host, TaskCompletionSource<bool> kafkaInitcompletionSource)
    {
        _host = host;
        _kafkaInitcompletionSource = kafkaInitcompletionSource;
    }

    public async Task StartAsync(CancellationToken ct)
    {
        using var adminClient = new AdminClientBuilder(
            new AdminClientConfig { BootstrapServers = _host })
            .Build();

        try
        {
            foreach (var (topicName, partitions) in _topicNames)
            {
                await adminClient.CreateTopicsAsync(new[]
                {
                    new TopicSpecification
                    {
                        Name = topicName,
                        NumPartitions = partitions,
                        ReplicationFactor = 1
                    }
                });

                Console.WriteLine($"Kafka topic '{topicName}' created successfully.");
            }

            await Task.Delay(TimeSpan.FromSeconds(5), ct);
        }
        catch (CreateTopicsException ex)
        {
            Console.WriteLine($"Error creating Kafka topic: {ex.Results[0].Error.Reason}");
        }
        finally
        {
            _kafkaInitcompletionSource.SetResult(true);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
        => Task.CompletedTask;
}