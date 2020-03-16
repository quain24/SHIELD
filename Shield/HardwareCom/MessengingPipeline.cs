using Shield.HardwareCom.Factories;
using Shield.HardwareCom.MessageProcessing;
using Shield.HardwareCom.Models;
using System;
using System.Collections.Concurrent;
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

        private readonly ConcurrentDictionary<string, IMessageModel> _internalStorage = new ConcurrentDictionary<string, IMessageModel>(StringComparer.InvariantCultureIgnoreCase);
        private readonly ConcurrentDictionary<string, IMessageModel> _sentMessages = new ConcurrentDictionary<string, IMessageModel>(StringComparer.InvariantCultureIgnoreCase);

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
            _incomingMessageProcessor.SwitchSourceCollectionTo(commandIngester.GetReceivedMessages());
        }

        public bool IsOpen => _messenger.IsOpen;

        public void Start()
        {
            _messenger.Open();

            _messenger.StartReceiveingAsync().ConfigureAwait(false);
            Task.Run(() => _commandIngester.StartProcessingCommands()).ConfigureAwait(false);
            Task.Run(() => _incomingMessageProcessor.StartProcessingMessagesContinous()).ConfigureAwait(false);
            Task.Run(async () => await HandleIncoming().ConfigureAwait(false));
            Task.Run(async () => await _commandIngester.StartTimeoutCheckAsync().ConfigureAwait(false));
            Task.Run(async () => await _confirmationTimeoutChecker.CheckUnconfirmedMessagesContinousAsync().ConfigureAwait(false));
        }

        public void Stop()
        {
            _incomingMessageProcessor.StopProcessingMessages();
            _commandIngester.StopTimeoutCheck();
            _commandIngester.StopProcessingCommands();
            _messenger.StopReceiving();
            _messenger.Close();
        }

        //TODO ended work here - creation and storage of confirmations
        private async Task HandleIncoming()
        {
            while (true)
            {
                IMessageModel receivedMessage = _incomingMessageProcessor.GetProcessedMessages().Take();
                if (!IsConfirmation(receivedMessage))
                {
                    IMessageModel confirmation = CreateConfirmationOf(receivedMessage);
                    if (await SendAsync(confirmation).ConfigureAwait(false))
                        _sentMessages.TryAdd(confirmation.Id, confirmation);
                    else
                        throw new Exception("Could not send confirmation.");
                }
                else
                {
                    _confirmationTimeoutChecker.AddConfirmation(receivedMessage);
                }
                _internalStorage.TryAdd(receivedMessage.Id, receivedMessage);
            }
        }

        public Task<bool> SendAsync(IMessageModel message)
        {
            _confirmationTimeoutChecker.AddToCheckingQueue(message);
            return _messenger.SendAsync(message);
        }

        private IMessageModel CreateConfirmationOf(IMessageModel message) =>
            _confirmationFactory.GenetateConfirmationOf(message);

        private bool IsConfirmation(IMessageModel message) =>
            message.Type == Enums.MessageType.Confirmation;

        public BlockingCollection<IMessageModel> GetReceivedMessages() => _incomingMessageProcessor.GetProcessedMessages();
    }
}