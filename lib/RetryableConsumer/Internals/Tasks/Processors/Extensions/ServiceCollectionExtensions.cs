using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RetryableConsumer.Abstractions.Handlers;
using RetryableConsumer.Internals.Channels.Strategy;
using RetryableConsumer.Internals.Registration.Configs;

namespace RetryableConsumer.Internals.Tasks.Processors.Extensions;

internal static class ServiceCollectionExtensions
{
    internal static void RegisterMainProcessorTasks<TKey, TValue, THandler>(
        this IServiceCollection services,
        ServiceProvider serviceProvider,
        RegistrationConfig config,
        IChannelStrategy<TKey, TValue> channelStrategy)
        where THandler : IHandler<TKey, TValue>
    {
        var retryConfig = config.Retries.FirstOrDefault();
        var dlqConfig = config.Dlq;
        
        var mainChannelReader = channelStrategy.GetMainChannel().Channel.Reader;
        var commitChannelWriter = channelStrategy.GetCommitChannel().Channel.Writer;
        var retryChannelWriter = retryConfig != null
            ? channelStrategy.GetRetryChannel(retryConfig.Topic)?.Channel.Writer
            : null;
        var dlqChannelWriter = dlqConfig != null
            ? channelStrategy.GetDlqChannel()?.Channel.Writer
            : null;

        for(var i=0; i < config.Main.ConcurrencyDegree; i++)
        {
            var id = "MainProcessorTask-" + i;
            

            services.AddSingleton<ITask>(prov =>
                ActivatorUtilities.CreateInstance<MainProcessorTask<TKey, TValue>>(
                    prov,
                    id,
                    ActivatorUtilities.CreateInstance<THandler>(serviceProvider),
                    mainChannelReader,
                    commitChannelWriter,
                    retryChannelWriter,
                    dlqChannelWriter)
            );
        }
        
    }
}