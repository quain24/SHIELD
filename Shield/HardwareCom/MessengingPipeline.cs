using Shield.HardwareCom.CommandProcessing;
using Shield.HardwareCom.Factories;
using Shield.HardwareCom.MessageProcessing;
using Shield.HardwareCom.Models;
using Shield.Helpers;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Shield.HardwareCom
{
    public class MessengingPipeline
    {
        private readonly IConfirmationTimeoutChecker _confirmationTimeoutChecker;
        private readonly ICompletitionTimeoutChecker _completitionTimeoutChecker;
        private readonly IIncomingMessageProcessor _incomingMessageProcessor;
        private readonly IConfirmationFactory _confirmationFactory;
        private readonly ICommandIngester _commandIngester;
        private readonly IIdGenerator _idGenerator;
        private readonly IMessenger _messenger;

        private readonly ConcurrentDictionary<string, IMessageModel> _receivedMessages = new ConcurrentDictionary<string, IMessageModel>(StringComparer.InvariantCultureIgnoreCase);
        private readonly ConcurrentDictionary<string, IMessageModel> _sentMessages = new ConcurrentDictionary<string, IMessageModel>(StringComparer.InvariantCultureIgnoreCase);
        private readonly BlockingCollection<IMessageModel> _forGUITemporary = new BlockingCollection<IMessageModel>();

        private CancellationTokenSource _handleNewMessagesCTS = new CancellationTokenSource();

        public MessengingPipeline(IMessengingPipelineContext context)
        {
            if (context is null)
                throw new ArgumentNullException(nameof(context));

            _confirmationTimeoutChecker = context.ConfirmationTimeoutChecker;
            _completitionTimeoutChecker = context.CompletitionTimeoutChecker;
            _confirmationFactory = context.ConfirmationFactory;
            _incomingMessageProcessor = context.Processor;
            _commandIngester = context.Ingester;
            _idGenerator = context.IdGenerator;
            _messenger = context.Messenger;

            _commandIngester.SwitchSourceCollectionTo(_messenger.GetReceivedCommands());
            _incomingMessageProcessor.SwitchSourceCollectionTo(_commandIngester.GetReceivedMessages());
        }

        public bool IsOpen => _messenger.IsOpen;

        public void Open()
        {
            _messenger.Open();

            _messenger.StartReceiveingAsync().ConfigureAwait(false);
            Task.Run(() => _commandIngester.StartProcessingCommands()).ConfigureAwait(false);
            Task.Run(() => _incomingMessageProcessor.StartProcessingMessagesContinous()).ConfigureAwait(false);
            Task.Run(async () => await _completitionTimeoutChecker.StartTimeoutCheckAsync().ConfigureAwait(false));
            Task.Run(async () => await _confirmationTimeoutChecker.CheckUnconfirmedMessagesContinousAsync().ConfigureAwait(false));
            Task.Run(async () => await HandleIncoming().ConfigureAwait(false));
        }

        public void Close()
        {
            CancelHandleIncoming();
            _confirmationTimeoutChecker.StopCheckingUnconfirmedMessages();
            _incomingMessageProcessor.StopProcessingMessages();
            _completitionTimeoutChecker.StopTimeoutCheck();
            _commandIngester.StopProcessingCommands();
            _messenger.StopReceiving();
            _messenger.Close();
        }

        private void CancelHandleIncoming()
        {
            _handleNewMessagesCTS.Cancel();
            _handleNewMessagesCTS = new CancellationTokenSource();
        }

        private async Task HandleIncoming()
        {
            while (!_handleNewMessagesCTS.IsCancellationRequested)
            {
                IMessageModel receivedMessage = GetNextReceivedMessage();
                receivedMessage.IsTransfered = true;

                if (IsConfirmation(receivedMessage))
                    HandleReceivedConfirmation(receivedMessage);
                else
                    await HandleReceivedMessage(receivedMessage).ConfigureAwait(false);
            }
        }

        private IMessageModel GetNextReceivedMessage() =>
            _incomingMessageProcessor.GetProcessedMessages().Take(_handleNewMessagesCTS.Token);

        private bool IsConfirmation(IMessageModel message) =>
            message.Type == Enums.MessageType.Confirmation;

        private void HandleReceivedConfirmation(IMessageModel confirmation)
        {
            _confirmationTimeoutChecker.AddConfirmation(confirmation);
            _receivedMessages.TryAdd(confirmation.Id, confirmation);
        }

        private async Task HandleReceivedMessage(IMessageModel message)
        {
            _receivedMessages.TryAdd(message.Id, message);
            _forGUITemporary.Add(message);
            await SendConfirmationOfAsync(message).ConfigureAwait(false);
        }

        private async Task SendConfirmationOfAsync(IMessageModel message)
        {
            IMessageModel confirmation = _confirmationFactory.GenetateConfirmationOf(message);
            if (await SendAsync(confirmation).ConfigureAwait(false))
                _sentMessages.TryAdd(confirmation.Id, confirmation);
            else
                throw new Exception("Could not send confirmation.");
        }

        public async Task<bool> SendAsync(IMessageModel message)
        {
            if (!IsOpen || message is null)
                return false;

            if (string.IsNullOrWhiteSpace(message.Id))
                message.Id = _idGenerator.GetNewID();
            if (message.Timestamp <= 0)
                message.Timestamp = Timestamp.TimestampNow;

            if (!IsConfirmation(message))
                AddToConfirmationTimeoutChecking(message);

            if (await _messenger.SendAsync(message).ConfigureAwait(false))
            {
                _sentMessages.TryAdd(message.Id, message);
                message.IsTransfered = true;
                return true;
            }
            else
                return false;
        }

        private void AddToConfirmationTimeoutChecking(IMessageModel confirmation)
        {
            _confirmationTimeoutChecker.AddToCheckingQueue(confirmation);
        }

        public BlockingCollection<IMessageModel> GetReceivedMessages() => _forGUITemporary;
    }
}