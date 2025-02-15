namespace RetryableConsumer.Internals.Channels;

internal enum ChannelType
{
    MainConsumer,
    MainConsumerCommit,
    RetryProducer,
    DlqProducer,
    RetryConsumer,
    RetryConsumerCommit
}