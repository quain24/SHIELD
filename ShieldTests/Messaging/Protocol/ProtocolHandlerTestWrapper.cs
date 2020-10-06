using Shield.Messaging.Commands;
using Shield.Messaging.DeviceHandler;
using Shield.Messaging.Protocol;
using CommandTranslator = Shield.Messaging.Protocol.CommandTranslator;

namespace ShieldTests.Messaging.Protocol
{
    public class ProtocolHandlerTestWrapper : ProtocolHandler
    {
        public ProtocolHandlerTestWrapper(IDeviceHandler deviceHandler, ConfirmationFactory confirmationFactory, ReplyFactory replyFactory, OrderFactory orderFactory, CommandTranslator commandTranslator, ResponseAwaiterDispatch awaiterDispatch)
            : base(deviceHandler, confirmationFactory, replyFactory, orderFactory, commandTranslator, awaiterDispatch)
        {
        }

        public new void OnCommandReceived(object sender, ICommand command)
        {
            base.OnCommandReceived(sender, command);
        }
    }
}