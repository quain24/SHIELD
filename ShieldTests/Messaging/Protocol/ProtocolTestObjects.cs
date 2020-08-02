using Shield.Messaging.Commands.States;
using Shield.Messaging.Protocol;
using Shield.Timestamps;

namespace ShieldTests.Messaging.Protocol
{
    public static class ProtocolTestObjects
    {

        public static int ConfirmationTimeout = 1000;
        public static int ReplyTimeout = 1000;
        public static Order GetNormalOrder()
        {
            return new Order("preciseOrder", "ARecipientMethod", "ID01", TimestampFactory.Timestamp, "TypicalData");
        }

        public static Confirmation GetNormalConfirmation()
        {
            return new Confirmation("ID01", ErrorState.Unchecked().Valid(), TimestampFactory.Timestamp);
        }

        public static Reply GetNormalReply()
        {
            return new Reply("ID01", TimestampFactory.Timestamp, "DataFromReply");
        }
    }
}