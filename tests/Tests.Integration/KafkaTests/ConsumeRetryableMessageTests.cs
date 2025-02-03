using System.Net.Http.Json;
using Confluent.Kafka;
using Harness.Models;
using Tests.Integration.Tests.Fixtures.App;
using Tests.Integration.Tests.Fixtures.Kafka;
using Xunit.Abstractions;

namespace Tests.Integration.KafkaTests;

[Collection("AppCollection")]
public class ConsumeRetryableMessageTests
{
    private readonly TimeSpan _retryDelay = TimeSpan.FromSeconds(5); 
    private readonly HttpClient _httpClient;

    public ConsumeRetryableMessageTests(
        AppFixture appFixture)
    {
        _httpClient = appFixture.HttpClient!;
    }

    [Fact]
    public async Task SuccessMessage_IsConsumed()
    {
        // Arrange
        var key = Guid.NewGuid().ToString();
        var inputValue = $"{key} success";
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
        var msg = await GetKafkaMessage("test-topic", host, testGroupId, inputValue, TimeSpan.FromSeconds(1));
        Assert.NotNull(msg);
        Assert.Equal(msg, inputValue);
        
        // Retry topic 1 is not produced
        msg = await GetKafkaMessage("test-topic-retry1", host, testGroupId, inputValue, _retryDelay * 2);
        Assert.Null(msg);
        
        // Retry topic 2 is not produced
        msg = await GetKafkaMessage("test-topic-retry2", host, testGroupId, inputValue, _retryDelay * 2);
        Assert.Null(msg);
        
        // Dlq is not produced
        msg = await GetKafkaMessage("test-topic-dlq", host, testGroupId, inputValue, _retryDelay * 2);
        Assert.Null(msg);
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
        var msg = await GetKafkaMessage("test-topic", host, testGroupId, inputValue, TimeSpan.FromSeconds(1));
        Assert.NotNull(msg);
        Assert.Equal(msg, inputValue);
        
        // Retry Topic 1
        for (var i = 0; i < 2; i++)
        {
            msg = await GetKafkaMessage("test-topic-retry1", host, testGroupId, inputValue, _retryDelay * 2);
            Assert.NotNull(msg);
            Assert.Equal(msg, inputValue);
        }
        // Third time in retry topic 1 should not be consumed
        msg = await GetKafkaMessage("test-topic-retry1", host, testGroupId, inputValue, _retryDelay * 2);
        Assert.Null(msg);
        
        // Retry Topic 2
        for (var i = 0; i < 2; i++)
        {
            msg = await GetKafkaMessage("test-topic-retry2", host, testGroupId, inputValue, _retryDelay * 2);
            Assert.NotNull(msg);
            Assert.Equal(msg, inputValue);
        }
        // Third time in retry topic 2 should not be consumed
        msg = await GetKafkaMessage("test-topic-retry2", host, testGroupId, inputValue, _retryDelay * 2);
        Assert.Null(msg);
        
        // Dlq
        msg = await GetKafkaMessage("test-topic-dlq", host, testGroupId, inputValue, _retryDelay * 2);
        Assert.NotNull(msg);
        Assert.Equal(msg, inputValue);
        // Dlq is produced only once
        msg = await GetKafkaMessage("test-topic-dlq", host, testGroupId, inputValue, _retryDelay * 2);
        Assert.Null(msg);
        
    }

    [Theory]
    [InlineData("dlq")]
    [InlineData("exception")]
    public async Task DlqMessage_ShouldGoDirectlyToDlqTopic(string messageType)
    {
        // Arrange
        var key = Guid.NewGuid().ToString();
        var inputValue = $"{key} {messageType}";
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
        var msg = await GetKafkaMessage("test-topic", host, testGroupId, inputValue, TimeSpan.FromSeconds(1));
        Assert.NotNull(msg);
        Assert.Equal(msg, inputValue);
        
        // Dlq
        msg = await GetKafkaMessage("test-topic-dlq", host, testGroupId, inputValue, _retryDelay * 2);
        Assert.NotNull(msg);
        Assert.Equal(msg, inputValue);
        // Dlq is produced only once
        msg = await GetKafkaMessage("test-topic-dlq", host, testGroupId, inputValue, _retryDelay * 2);
        Assert.Null(msg);
        
        // retry 1 is not produced
        msg = await GetKafkaMessage("test-topic-retry1", host, testGroupId, inputValue, _retryDelay * 2);
        Assert.Null(msg);
        
        // retry 2 is not produced
        msg = await GetKafkaMessage("test-topic-retry2", host, testGroupId, inputValue, _retryDelay * 2);
        Assert.Null(msg);
        
    }
    private async Task<string?> GetKafkaMessage(
        string topic, 
        string host, 
        string groupId,
        string inputValue,
        TimeSpan consumeTimeOut)
    {
        var consumer = KafkaFixture.CreateConsumer(host, groupId);
        consumer.Subscribe(topic);
        
        var msg = "";
        while (msg != null && msg != inputValue)
        {
            try
            {
                var resultMessage = await Task.Run(() => consumer.Consume(consumeTimeOut));
                if (resultMessage != null)
                {
                    msg = resultMessage?.Message.Value.Value;
                    consumer.Commit();    
                }
                else
                {
                    msg = null;
                }
            }
            catch (ConsumeException)
            {
                msg = null;
            }
        }
        
        consumer.Close();
        
        return msg;
    }
}