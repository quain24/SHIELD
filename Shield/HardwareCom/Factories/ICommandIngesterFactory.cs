using Shield.HardwareCom.MessageProcessing;

namespace Shield.HardwareCom.Factories
{
    public interface ICommandIngesterFactory
    {
        ICommandIngesterAlt GetIngesterUsing(int timeout);
    }
}