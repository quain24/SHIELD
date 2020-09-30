using System.Collections.Generic;
using System.Diagnostics;
using Moq;
using Shield.Messaging.Commands;
using Shield.Messaging.Commands.States;
using Shield.Messaging.DeviceHandler;
using Shield.Messaging.Protocol;
using Shield.Messaging.RawData;
using ShieldTests.Messaging.Commands;
using ShieldTests.Messaging.Commands.Parts;
using System.Linq;
using System.Threading.Tasks;
using Shield.Timestamps;
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
        public CommandTranslator Translator;

        private void Setup()
        {
            var partFactory = PartFactoryTestObjects.GetAlwaysValidPartFactory();
            CommandFactory = CommandsTestObjects.GetProperAlwaysValidCommandFactory();
            var idGenerator = new IdGenerator(4);
            var orderFactory = new OrderFactory(idGenerator);
            var replyFactory = new ReplyFactory(idGenerator);
            var confirmationFactory = new ConfirmationFactory();

            Mock<IDataStreamSplitter> splitter = new Mock<IDataStreamSplitter>();
            Mock<ICommunicationDeviceAsync> comDevice = new Mock<ICommunicationDeviceAsync>();
            var translator = new CommandTranslator(new OrderCommandTranslator(partFactory, CommandFactory, orderFactory),
                new ReplyCommandTranslator(partFactory, CommandFactory, replyFactory),
                new ConfirmationCommandTranslator(partFactory, CommandFactory, confirmationFactory), new ErrorCommandTranslator());
            Translator = translator;

            Mock<IDeviceHandler> handler = new Mock<IDeviceHandler>();
            handler.Setup(h => h.SendAsync(It.IsAny<ICommand>())).ReturnsAsync(true);

            DeviceMoq = handler;
            ResponseAwaiterDispatch = ResponseAwaiterDispatchTestObjects.GetProperResponseAwaiterDispatch();
            ProtocolHandler = new ProtocolHandlerTestWrapper(handler.Object, confirmationFactory,  translator, ResponseAwaiterDispatch);
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
            Assert.True(receivedOrder.Data.GetDataInTransmittableFormat() == testCommand.Data.ToString());
        }

        [Fact()]
        public void When_informed_by_DeviceHandler_about_new_received_confirmation_type_command_provides_proper_confirmation_to_ResponseAwaiterDispatch()
        {
            Order receivedOrder = null;
            ProtocolHandler.OrderReceived += (_, order) => receivedOrder = order;

            RaiseNewCommandEventOnDevice(CommandsTestObjects.GetProperTestCommand_order("ID01"));
            RaiseNewCommandEventOnDevice(CommandsTestObjects.GetProperTestCommand_confirmation("ID01"));
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
            
            RaiseNewCommandEventOnDevice(CommandsTestObjects.GetProperTestCommand_order("ID01"));
            RaiseNewCommandEventOnDevice(CommandsTestObjects.GetProperTestCommand_reply("ID01"));
            var receivedReply = ResponseAwaiterDispatch.ReplyTo(receivedOrder);

            Assert.True(receivedOrder.ID == receivedReply.ReplyTo);
            Assert.True(CommandsTestObjects.GetProperTestCommand_reply().Data.ToString() == receivedReply.Data);
        }

        [Fact()]
        public void When_informed_by_DeviceHandler_about_new_received_invalid_command_raises_IncomingCommunicationErrorOccured_ErrorMessage()
        {
            ErrorMessage receivedErrorMsg = null;
            ProtocolHandler.IncomingCommunicationErrorOccurred += (_, error) => receivedErrorMsg = error;

            RaiseNewCommandEventOnDevice(CommandsTestObjects.GetInvalidCommand());

            Assert.IsType<ErrorMessage>(receivedErrorMsg);
            Assert.True(receivedErrorMsg.ErrorState == CommandsTestObjects.GetInvalidCommand().ErrorState);
            Assert.Equal(receivedErrorMsg.Data, CommandsTestObjects.GetInvalidCommand().Select(p => p.GetType().Name + " | " + p.ToString()));
        }

        [Fact()]
        public async Task Should_return_confirmation_when_proper_Order_was_sent()
        {
            var result = await ProtocolHandler.SendAsync(ProtocolTestObjects.GetNormalOrder());

            Assert.True(result is Confirmation);
        }

        [Fact()]
        public async Task Should_return_true_when_proper_Confirmation_was_sent()
        {
            bool result = await ProtocolHandler.SendAsync(ProtocolTestObjects.GetNormalConfirmation());

            Assert.True(result);
        }

        [Fact()]
        public async Task Should_return_confirmation_when_proper_Reply_was_sent()
        {
            var result = await ProtocolHandler.SendAsync(ProtocolTestObjects.GetNormalReply());

            Assert.True(result is Confirmation);
        }

        [Fact()]
        public void Should_return_IAwaitingDispatch_when_WasOrder_is_called()
        {
            Assert.IsAssignableFrom<IAwaitingDispatch>(ProtocolHandler.Order());
        }

        [Fact()]
        public void Should_return_IRetrievingDispatch_when_Retrieve_id_called()
        {
            Assert.IsAssignableFrom<IRetrievingDispatch>(ProtocolHandler.Retrieve());
        }

        [Fact()]
        public async Task ProtocolHandlerPerformanceTest()
        {
            var data = ProtocolHandler;

            var a = TimestampFactory.Timestamp;
            Debug.WriteLine(TimestampFactory.Timestamp);

            for (int i = 0; i < 10000; i++)
            {
                data.OnCommandReceived(this, CommandsTestObjects.GetProperTestCommand_confirmation("id" + i));
                //data.AddICommand(CommandsTestObjects.GetProperTestCommand_order("id" + i));
            }

            Debug.WriteLine(TimestampFactory.Timestamp);
            Debug.WriteLine(TimestampFactory.Timestamp.Difference(a));
            var ll = new List<Task<bool>>();

            for (int i = 0; i < 1000; i++)
            {
                if (i % 50 == 0)
                    ll.Add(ProtocolHandler.Order().WasConfirmedInTimeAsync(ProtocolTestObjects.GetNormalOrder("id" + i)));
            }

            await Task.WhenAll(ll);
            Debug.WriteLine(TimestampFactory.Timestamp);
            Debug.WriteLine(TimestampFactory.Timestamp.Difference(a));
            Debug.WriteLine("end");

        }
    }
}