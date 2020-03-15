using Shield.HardwareCom.MessageProcessing;

namespace Shield.HardwareCom.Factories
{
    public interface ITimeoutCheckFactory
    {
        ITimeoutCheck GetTimeoutCheckWithTimeoutSetTo(int milliseconds);
    }
}