using System.Collections.Concurrent;
using Shield.Messaging.Commands;
using Shield.Messaging.DeviceHandler;
using System.Threading.Tasks.Dataflow;
using Shield.Messaging.Extensions;

namespace Shield.Messaging.Protocol
{
    public class MessagingPipeline
    {
        private readonly DeviceHandlerContext _deviceHandlerContext;
        private readonly ProtocolHandler _protocolHandler;
        private readonly ResponseAwaiterDispatch _responseAwaiterDispatch;
        private readonly CommandTranslator _commandTranslator;
        public readonly ConcurrentQueue<Order> _orderBuffer = new ConcurrentQueue<Order>();
        public readonly ConcurrentQueue<Reply> _replyBuffer = new ConcurrentQueue<Reply>();
        public readonly ConcurrentQueue<Confirmation> _confirmationBuffer = new ConcurrentQueue<Confirmation>();
        public readonly ConcurrentQueue<ErrorMessage> _errorBuffer = new ConcurrentQueue<ErrorMessage>();
        public BufferBlock<ICommand> _buffer;


        public MessagingPipeline(DeviceHandlerContext deviceHandlerContext, ProtocolHandler protocolHandler, CommandTranslator commandTranslator, ResponseAwaiterDispatch responseAwaiterDispatch)
        {
            _deviceHandlerContext = deviceHandlerContext;
            _protocolHandler = protocolHandler;
            _responseAwaiterDispatch = responseAwaiterDispatch;
            _commandTranslator = commandTranslator;

            SetupPipeline();
        }

        private void SetupPipeline()
        {
            var linkOptions = new DataflowLinkOptions { PropagateCompletion = true };
            var executionOptions = new ExecutionDataflowBlockOptions( );

            var buffer = new BufferBlock<ICommand>();
            _buffer = buffer;
            var toOrder = new TransformBlock<ICommand, Order>(_commandTranslator.TranslateToOrder, executionOptions);
            var toConfirmation = new TransformBlock<ICommand, Confirmation>(_commandTranslator.TranslateToConfirmation, executionOptions);
            var toReply = new TransformBlock<ICommand, Reply>(_commandTranslator.TranslateToReply, executionOptions);
            var toError = new TransformBlock<ICommand, ErrorMessage>(_commandTranslator.TranslateToErrorMessage, executionOptions);

            var OrderOutput = new ActionBlock<Order>(o => _orderBuffer.Enqueue(o), executionOptions);
            var ConfirmationOutput = new ActionBlock<Confirmation>(c => _responseAwaiterDispatch.AddResponse(c), executionOptions);
            var ReplyOutput = new ActionBlock<Reply>(r => _replyBuffer.Enqueue(r), executionOptions);
            var ErrorOutput = new ActionBlock<ErrorMessage>(e => _errorBuffer.Enqueue(e), executionOptions);

            buffer.LinkTo(toOrder, linkOptions,
                command => command.IsValid && !command.IsConfirmation() && !command.IsReply());
            buffer.LinkTo(toConfirmation, linkOptions, command => command.IsValid && command.IsConfirmation());
            buffer.LinkTo(toReply, linkOptions, command => command.IsValid && command.IsReply());
            buffer.LinkTo(toError, linkOptions, command => !command.IsValid);

            toOrder.LinkTo(OrderOutput, linkOptions);
            toConfirmation.LinkTo(ConfirmationOutput, linkOptions);
            toReply.LinkTo(ReplyOutput, linkOptions);
            toError.LinkTo(ErrorOutput, linkOptions);
        }

        public void AddICommand(ICommand command)
        {
            _buffer.Post(command);
        }
    }
}