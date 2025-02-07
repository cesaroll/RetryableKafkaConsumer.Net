namespace RetryableConsumer.Internals.Channels;

internal enum ChannelType
{
    Main,
    Commit,
    Retry,
    Dlq 
}