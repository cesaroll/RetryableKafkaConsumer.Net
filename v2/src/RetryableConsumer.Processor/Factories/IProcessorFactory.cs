using RetryableConsumer.Domain.Configs;
using RetryableConsumer.Processor.Processors;

namespace RetryableConsumer.Processor.Factories;

public interface IProcessorFactory
{
    IList<IProcessor> CreateProcessors(RegistrationConfig config);
}