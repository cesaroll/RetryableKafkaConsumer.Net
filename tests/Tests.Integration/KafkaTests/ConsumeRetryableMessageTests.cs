using System.Net.Http.Json;
using Confluent.Kafka;
using Harness.Models;
using Tests.Integration.Tests.Fixtures.App;
using Tests.Integration.Tests.Fixtures.Kafka;

namespace Tests.Integration.KafkaTests;

[Collection("AppCollection")]
public class ConsumeRetryableMessageTests
{
    private readonly TimeSpan _retryDelay = TimeSpan.FromSeconds(5); 
    private readonly HttpClient _httpClient;
    

    public ConsumeRetryableMessageTests(AppFixture appFixture)
    {
        _httpClient = appFixture.HttpClient;
    }

    [Fact]
    public async Task RetryableMessage_IsRetriedUntilDlq()
    {
        // Arrange
        var key = Guid.NewGuid().ToString();
        var inputValue = $"{key} retry";
        var inputMessage = new TestMessage()
        {
            Key = key,
            Value = inputValue
        };

        var host = await KafkaFixture.GetKafkaBrokerHost(_httpClient);
        var testGroupId = Guid.NewGuid().ToString();

        // Act
        await _httpClient.PostAsync("/kafka/produce", JsonContent.Create(inputMessage));

        // Assert
        
        // Main Topic
        var msg = await GetKafkaMessage("test-topic", host, testGroupId);
        Assert.Equal(msg, inputValue);
        
        // Retry Topic 1
        for (var i = 0; i < 1; i++)
        {
            msg = await GetKafkaMessage("test-topic-retry1", host, testGroupId);
            Assert.Equal(msg, inputValue);
        }
        
        // Retry Topic 2
        for (var i = 0; i < 1; i++)
        {
            msg = await GetKafkaMessage("test-topic-retry2", host, testGroupId);
            Assert.Equal(msg, inputValue);
        }
        
        // Dlq
        msg = await GetKafkaMessage("test-topic-dlq", host, testGroupId);
        Assert.Equal(msg, inputValue);
        
    }

    private async Task<string> GetKafkaMessage(
        string topic, string host, string groupId)
    {
        var consumeTimeOut = _retryDelay * 3;
        var consumer = KafkaFixture.CreateConsumer(host, groupId);
        consumer.Subscribe(topic);
        var resultMessage = await Task.Run(() => consumer.Consume(consumeTimeOut));
        Assert.NotNull(resultMessage);
        Assert.NotNull(resultMessage.Message.Value);
        Assert.NotNull(resultMessage.Message.Value.Value);
        var msg = resultMessage.Message.Value.Value;
        consumer.Commit();
        consumer.Close();
        
        return msg;
    }
}