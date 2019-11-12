using Shield.Enums;
using Shield.HardwareCom.Models;

namespace Shield.HardwareCom
{
    public interface IMessageInfoAndErrorChecks
    {
        long CompletitionTimeout { get; set; }
        long ConfirmationTimeout { get; set; }

        bool CompletitionTimeoutExceeded(IMessageModel message);
        bool ConfirmationTimeoutExceeded(IMessageModel message);
        MessageErrors DecodingErrorsIn(IMessageModel message);
        IncomingMessageType DetectType(IMessageModel message);
        bool IsPatternCorrect(IMessageModel message);
        bool IsCompleted(IMessageModel message);
        bool InCompletitionWindow(IMessageModel message);
        bool InConfirmationWindow(IMessageModel message);
        bool IsCompleted(IMessageHWComModel message);
        bool IsPatternCorrect(IMessageHWComModel message);
        bool ConfirmationTimeoutExceeded(IMessageHWComModel message);
        bool CompletitionTimeoutExceeded(IMessageHWComModel message);
    }
}