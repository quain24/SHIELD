using Shield.CommonInterfaces;

namespace Shield.HardwareCom.Factories
{
    public interface IMessengingPipelineContextFactory
    {
        IMessagingPipelineContext GetContextFor(ICommunicationDevice device);
    }
}