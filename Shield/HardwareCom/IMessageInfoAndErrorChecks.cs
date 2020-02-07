using Shield.Enums;
using Shield.HardwareCom.Models;

namespace Shield.HardwareCom
{
    public interface IMessageInfoAndErrorChecks
    {
        long CompletitionTimeout { get; set; }
        long ConfirmationTimeout { get; set; }

        bool IsCompletitionTimeoutExceeded(IMessageModel message);

        bool IsConfirmationTimeoutExceeded(IMessageModel message);

        MessageErrors DecodingErrorsIn(IMessageModel message);

        Errors DecodingErrorsIn(IMessageHWComModel message);

        IncomingMessageType DetectTypeOf(IMessageModel message);

        MessageType DetectTypeOf(IMessageHWComModel message);

        bool IsPatternCorrect(IMessageModel message);

        bool IsCompleted(IMessageModel message);

        bool InCompletitionWindow(IMessageModel message);

        bool InConfirmationWindow(IMessageModel message);

        bool IsCompleted(IMessageHWComModel message);

        bool IsPatternCorrect(IMessageHWComModel message);

        bool IsConfirmationTimeoutExceeded(IMessageHWComModel message);

        bool IsCompletitionTimeoutExceeded(IMessageHWComModel message);
    }
}