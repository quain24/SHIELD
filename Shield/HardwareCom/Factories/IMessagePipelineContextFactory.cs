using Shield.CommonInterfaces;

namespace Shield.HardwareCom.Factories
{
    public interface IMessagePipelineContextFactory
    {
        IMessagePipelineContext GetContextFor(ICommunicationDevice device);
    }
}