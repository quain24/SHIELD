using Shield.CommonInterfaces;

namespace Shield.HardwareCom.Factories
{
    public interface IMessengingPipelineFactory
    {
        IMessengingPipeline GetPipelineFor(ICommunicationDevice device);
    }
}