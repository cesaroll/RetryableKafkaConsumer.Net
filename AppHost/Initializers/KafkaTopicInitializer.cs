using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Microsoft.Extensions.Hosting;

namespace AppHost.Initializers;

public class KafkaTopicInitializer : IHostedService
{
    private readonly string _bootstrapServers = "localhost:9092";
    private readonly List<(string, int)> _topicNames =
    [
        ("test-topic", 3),
        ("test-topic-retry1", 1),
        ("test-topic-retry2", 1),
        ("test-topic-dlq", 1)
    ];
    
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var adminClient = new AdminClientBuilder(
            new AdminClientConfig { BootstrapServers = _bootstrapServers })
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
        }
        catch (CreateTopicsException ex)
        {
            Console.WriteLine($"Error creating Kafka topic: {ex.Results[0].Error.Reason}");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
        => Task.CompletedTask;
}