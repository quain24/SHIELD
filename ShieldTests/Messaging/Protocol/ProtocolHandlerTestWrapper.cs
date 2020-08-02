using Shield.Messaging.Commands;
using Shield.Messaging.DeviceHandler;
using Shield.Messaging.Protocol;
using CommandTranslator = Shield.Messaging.Protocol.CommandTranslator;

namespace ShieldTests.Messaging.Protocol
{
    public class ProtocolHandlerTestWrapper : ProtocolHandler
    {
        public ProtocolHandlerTestWrapper(IDeviceHandler deviceHandler, CommandTranslator commandTranslator, ResponseAwaiterDispatch awaiterDispatch) : base(deviceHandler, commandTranslator, awaiterDispatch)
        {
        }

        public new void OnCommandReceived(object sender, ICommand command)
        {
            base.OnCommandReceived(sender, command);
        }
    }
}