using Shield.Messaging.Commands.States;
using Shield.Messaging.Protocol;
using Shield.Timestamps;

namespace ShieldTests.Messaging.Protocol
{
    public static class ProtocolTestObjects
    {
        public static Order GetNormalOrder(string id)
        {
            return new Order("preciseOrder", "ARecipientMethod", id, TimestampFactory.Timestamp, "TypicalData");
        }
        public static Order GetNormalOrder()
        {
            return GetNormalOrder("ID01");
        }

        public static Confirmation GetNormalConfirmation(string id)
        {
            return new Confirmation(id, ErrorState.Unchecked().Valid(), TimestampFactory.Timestamp);
        }

        public static Confirmation GetNormalConfirmation()
        {
            return GetNormalConfirmation("ID01");
        }

        public static Reply GetNormalReply()
        {
            return new Reply("ID01", TimestampFactory.Timestamp, "DataFromReply");
        }

    }
}