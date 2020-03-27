using Shield.CommonInterfaces;

namespace Shield.HardwareCom.Factories
{
    public interface IMessengingPipelineContextFactory
    {
        IMessengingPipelineContext GetContextFor(ICommunicationDevice device);
    }
}