using Shield.HardwareCom.Factories;
using Shield.HardwareCom.MessageProcessing;
using Shield.HardwareCom.Models;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Shield.HardwareCom
{
    public class MessengingPipeline
    {
        private readonly IMessenger _messenger;
        private readonly ICommandIngester _commandIngester;
        private readonly IIncomingMessageProcessor _incomingMessageProcessor;
        private readonly IConfirmationTimeoutChecker _confirmationTimeoutChecker;
        private readonly IConfirmationFactory _confirmationFactory;

        private readonly ConcurrentDictionary<string, IMessageModel> _sentMessages = new ConcurrentDictionary<string, IMessageModel>(StringComparer.InvariantCultureIgnoreCase);
        private readonly ConcurrentDictionary<string, IMessageModel> _receivedMessages = new ConcurrentDictionary<string, IMessageModel>(StringComparer.InvariantCultureIgnoreCase);

        private CancellationTokenSource _handleNewMessagesCTS = new CancellationTokenSource();

        public event EventHandler<IMessageModel> OnMessageReceived;
        public event EventHandler<IMessageModel> OnConfirmationReceived;
        public event EventHandler<IMessageModel> OnMessageSent;
        public event EventHandler<IMessageModel> OnMessageWasConfirmed;
        public event EventHandler<IMessageModel> OnMessageConfirmationTimout;
        public event EventHandler<IMessageModel> OnMessageCompletitionTimout;

        // TODO handle events, how to wire them up to inform about completition, timeouts and similar.

        public MessengingPipeline(IMessenger messanger,
                                  ICommandIngester commandIngester,
                                  IIncomingMessageProcessor incomingMessageProcessor,
                                  IConfirmationTimeoutChecker confirmationTimeoutChecker,
                                  IConfirmationFactory confirmationFactory)
        {
            _messenger = messanger ?? throw new ArgumentNullException(nameof(messanger));
            _commandIngester = commandIngester ?? throw new ArgumentNullException(nameof(commandIngester));
            _incomingMessageProcessor = incomingMessageProcessor ?? throw new ArgumentNullException(nameof(incomingMessageProcessor));
            _confirmationTimeoutChecker = confirmationTimeoutChecker ?? throw new ArgumentNullException(nameof(confirmationTimeoutChecker));
            _confirmationFactory = confirmationFactory ?? throw new ArgumentNullException(nameof(confirmationFactory));

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
            Task.Run(async () => await _commandIngester.StartTimeoutCheckAsync().ConfigureAwait(false));
            Task.Run(async () => await _confirmationTimeoutChecker.CheckUnconfirmedMessagesContinousAsync().ConfigureAwait(false));
            Task.Run(async () => await HandleIncoming().ConfigureAwait(false));
        }

        public void Close()
        {
            CancelHandleIncoming();
            _confirmationTimeoutChecker.StopCheckingUnconfirmedMessages();
            _incomingMessageProcessor.StopProcessingMessages();
            _commandIngester.StopTimeoutCheck();
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

            _confirmationTimeoutChecker.AddToCheckingQueue(message);

            if (await _messenger.SendAsync(message).ConfigureAwait(false))
            {
                _sentMessages.TryAdd(message.Id, message);
                return true;
            }
            else
                return false;
        }

        public BlockingCollection<IMessageModel> GetReceivedMessages() => _incomingMessageProcessor.GetProcessedMessages();
    }
}