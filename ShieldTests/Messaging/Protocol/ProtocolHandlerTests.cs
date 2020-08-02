using Moq;
using Shield.Messaging.Commands;
using Shield.Messaging.Commands.Parts;
using Shield.Messaging.Commands.States;
using Shield.Messaging.DeviceHandler;
using Shield.Messaging.Protocol;
using Shield.Messaging.RawData;
using Shield.Timestamps;
using ShieldTests.Messaging.Commands;
using ShieldTests.Messaging.Commands.Parts;
using System;
using Xunit;
using Xunit.Abstractions;
using CommandTranslator = Shield.Messaging.Protocol.CommandTranslator;

namespace ShieldTests.Messaging.Protocol
{
    public class ProtocolHandlerTests
    {
        private readonly ITestOutputHelper _output;

        public ProtocolHandlerTests(ITestOutputHelper output)
        {
            _output = output;
            Setup();
        }

        public ProtocolHandlerTestWrapper ProtocolHandler;
        public Mock<IDeviceHandler> DeviceMoq;
        public CommandFactory CommandFactory;
        public ResponseAwaiterDispatch ResponseAwaiterDispatch;

        private void Setup()
        {
            var partFactory = PartFactoryTestObjects.GetAlwaysValidPartFactory();

            var commandFactoryAutofacAdapter = new CommandFactoryAutoFacAdapter(
                new Func<IPart, IPart, IPart, IPart, IPart, Timestamp, ICommand>((id, hostID, target, order, data, _) =>
                    new Command(id, hostID, target, order, data, TimestampFactory.Timestamp)));
            var commandFactory = new CommandFactory('*', partFactory, commandFactoryAutofacAdapter, new IdGenerator(4));

            Mock<IDataStreamSplitter> splitter = new Mock<IDataStreamSplitter>();
            Mock<ICommunicationDeviceAsync> comDevice = new Mock<ICommunicationDeviceAsync>();
            var translator = new CommandTranslator(new OrderCommandTranslator(partFactory, commandFactory),
                new ReplyCommandTranslator(partFactory, commandFactory),
                new ConfirmationCommandTranslator(partFactory, commandFactory), new ErrorCommandTranslator());

            Mock<IDeviceHandler> handler = new Mock<IDeviceHandler>();

            CommandFactory = commandFactory;
            DeviceMoq = handler;
            ResponseAwaiterDispatch = ResponseAwaiterDispatchTestObjects.GetProperResponseAwaiterDispatch();
            ProtocolHandler = new ProtocolHandlerTestWrapper(handler.Object, translator, ResponseAwaiterDispatch);
        }

        public void RaiseNewCommandEventOnDevice(ICommand command)
        {
            DeviceMoq.Raise(e => e.CommandReceived += ProtocolHandler.OnCommandReceived, null, command);
        }

        [Fact()]
        public void When_informed_by_DeviceHandler_about_new_received_order_type_command_provides_proper_order_by_event()
        {
            Order receivedOrder = null;
            ProtocolHandler.OrderReceived += (_, order) => receivedOrder = order;
            var testCommand = CommandsTestObjects.GetProperTestCommand_order();

            RaiseNewCommandEventOnDevice(testCommand);

            Assert.True(receivedOrder.ID == testCommand.ID.ToString());
            Assert.True(receivedOrder.Target == testCommand.Target.ToString());
            Assert.True(receivedOrder.ExactOrder == testCommand.Order.ToString());
            Assert.True(receivedOrder.Data == testCommand.Data.ToString());
        }

        [Fact()]
        public void When_informed_by_DeviceHandler_about_new_received_confirmation_type_command_provides_proper_confirmation_to_ResponseAwaiterDispatch()
        {
            Order receivedOrder = null;
            ProtocolHandler.OrderReceived += (_, order) => receivedOrder = order;

            RaiseNewCommandEventOnDevice(CommandsTestObjects.GetProperTestCommand_order());
            RaiseNewCommandEventOnDevice(CommandsTestObjects.GetProperTestCommand_confirmation());
            var receivedConfirmation = ResponseAwaiterDispatch.ConfirmationOf(receivedOrder);

            Assert.True(receivedOrder.ID == receivedConfirmation.Confirms);
            Assert.IsType<ErrorState>(receivedConfirmation.ContainedErrors);
            Assert.True(receivedConfirmation.IsValid);
            Assert.True(receivedConfirmation.ContainedErrors == ErrorState.Unchecked().Valid());
        }

        [Fact()]
        public void When_informed_by_DeviceHandler_about_new_received_reply_type_command_provides_proper_reply_to_ResponseAwaiterDispatch()
        {
            Order receivedOrder = null;
            ProtocolHandler.OrderReceived += (_, order) => receivedOrder = order;

            RaiseNewCommandEventOnDevice(CommandsTestObjects.GetProperTestCommand_order());
            RaiseNewCommandEventOnDevice(CommandsTestObjects.GetProperTestCommand_reply());
            var receivedReply = ResponseAwaiterDispatch.ReplyTo(receivedOrder);

            Assert.True(receivedOrder.ID == receivedReply.ReplysTo);
            Assert.True(CommandsTestObjects.GetProperTestCommand_reply().Data.ToString() == receivedReply.Data);
        }
    }
}