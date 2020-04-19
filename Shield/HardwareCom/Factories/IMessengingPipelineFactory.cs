using Shield.CommonInterfaces;

namespace Shield.HardwareCom.Factories
{
    public interface IMessengingPipelineFactory
    {
        IMessagingPipeline GetPipelineFor(ICommunicationDevice device);
    }
}