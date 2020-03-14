using Shield.CommonInterfaces;

namespace Shield.HardwareCom.Factories
{
    public interface IMessengingPipelineFactory
    {
        MessengingPipeline GetPipelineFor(ICommunicationDevice device);
    }
}