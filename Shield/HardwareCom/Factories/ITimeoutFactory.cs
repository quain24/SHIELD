using Shield.HardwareCom.MessageProcessing;

namespace Shield.HardwareCom.Factories
{
    public interface ITimeoutFactory
    {
        ITimeout CreateTimeoutWith(int milliseconds);
    }
}