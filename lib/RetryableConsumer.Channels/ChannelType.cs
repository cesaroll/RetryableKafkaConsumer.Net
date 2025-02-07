using RetryableConsumer.Domain.Configs;

namespace RetryableConsumer.Channels;

internal enum ChannelType
{
    Main,
    Commit,
    Retry,
    Dlq 
}