namespace RetryableKafkaConsumer.Consumers.Config;

internal record GenericTopicConfig(
    string Server,
    string Topic, 
    string? GroupId = null,
    TimeSpan? Delay = null,
    int MaxInTopicRetryAttempts = 0
);