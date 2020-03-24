using Shield.HardwareCom.Factories;
using Shield.HardwareCom.MessageProcessing;
using Shield.Helpers;

namespace Shield.HardwareCom
{
    public interface IMessagePipelineContext
    {
        ICompletitionTimeoutChecker CompletitionTimeoutChecker { get; }
        IConfirmationTimeoutChecker ConfirmationTimeoutChecker { get; }
        ICommandIngesterAlt Ingester { get; }
        IMessenger Messenger { get; }
        IIncomingMessageProcessor Processor { get; }
        IIdGenerator IdGenerator { get; }
        IConfirmationFactory ConfirmationFactory { get; }
    }
}