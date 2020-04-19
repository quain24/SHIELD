using Shield.HardwareCom.Models;
using Shield.Helpers;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Shield.HardwareCom
{
    public class MessagingPipeline : IMessagingPipeline, IDisposable
    {
        public event EventHandler<IMessageModel> MessageSent;

        public event EventHandler<IMessageModel> ConfirmationSent;

        public event EventHandler<IMessageModel> MessageReceived;

        public event EventHandler<IMessageModel> ConfirmationReceived;

        public event EventHandler<IMessageModel> SendingFailed;

        private readonly IMessengingPipelineContext _context;
        private readonly ConcurrentDictionary<string, IMessageModel> _receivedMessages = new ConcurrentDictionary<string, IMessageModel>(StringComparer.OrdinalIgnoreCase);
        private readonly ConcurrentDictionary<string, IMessageModel> _sentMessages = new ConcurrentDictionary<string, IMessageModel>(StringComparer.OrdinalIgnoreCase);
        private readonly ConcurrentDictionary<string, IMessageModel> _failedSendMessages = new ConcurrentDictionary<string, IMessageModel>(StringComparer.OrdinalIgnoreCase);
        private readonly BlockingCollection<IMessageModel> _forGUITemporary = new BlockingCollection<IMessageModel>();

        private CancellationTokenSource _handleNewMessagesCTS = new CancellationTokenSource();
        private bool _disposed = false;

        public bool IsOpen => _context?.Messenger?.IsOpen ?? false;

        // TODO refactoring and ordering / regions? 

        public MessagingPipeline(IMessengingPipelineContext context)
        {
            if (context is null)
                throw new ArgumentNullException(nameof(context));

            _context = context;

            SetUpCollections();
        }

        private void SetUpCollections()
        {
            _context.Ingester.SwitchSourceCollectionTo(_context.Messenger.GetReceivedCommands());
            _context.Processor.SwitchSourceCollectionTo(_context.Ingester.GetReceivedMessages());
        }

        public void Open()
        {
            _context.Messenger.Open();

            _context.Messenger.StartReceiveingAsync().ConfigureAwait(false);
            Task.Run(() => _context.Ingester.StartProcessingCommands()).ConfigureAwait(false);
            Task.Run(() => _context.Processor.StartProcessingMessagesContinous()).ConfigureAwait(false);
            Task.Run(async () => await _context.CompletitionTimeoutChecker.StartTimeoutCheckAsync().ConfigureAwait(false));
            Task.Run(async () => await _context.ConfirmationTimeoutChecker.CheckUnconfirmedMessagesContinousAsync().ConfigureAwait(false));
            Task.Run(async () => await HandleIncoming().ConfigureAwait(false));
        }

        public void Close()
        {
            CancelHandleIncoming();
            _context.ConfirmationTimeoutChecker.StopCheckingUnconfirmedMessages();
            _context.CompletitionTimeoutChecker.StopTimeoutCheck();
            _context.Processor.StopProcessingMessages();
            _context.Ingester.StopProcessingCommands();
            _context.Messenger.StopReceiving();
            _context.Messenger.Close();
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
            _context.Processor.GetProcessedMessages().Take(_handleNewMessagesCTS.Token);

        private bool IsConfirmation(IMessageModel message) =>
            message?.Type == Enums.MessageType.Confirmation;

        private void HandleReceivedConfirmation(IMessageModel confirmation)
        {
            _context.ConfirmationTimeoutChecker.AddConfirmation(confirmation);
            _receivedMessages.TryAdd(confirmation.Id, confirmation);
            OnConfirmationReceived(confirmation);
        }

        private async Task HandleReceivedMessage(IMessageModel message)
        {
            _receivedMessages.TryAdd(message.Id, message);
            _forGUITemporary.Add(message);
            OnMessageReceived(message);
            await SendConfirmationOfAsync(message).ConfigureAwait(false);
        }

        private async Task SendConfirmationOfAsync(IMessageModel message)
        {
            IMessageModel confirmation = _context.ConfirmationFactory.GenerateConfirmationOf(message);
            if (await SendAsync(confirmation).ConfigureAwait(false))
                _sentMessages.TryAdd(confirmation.Id, confirmation);
        }

        public async Task<bool> SendAsync(IMessageModel message)
        {
            if (!IsOpen || message is null)
                return false;

            if (string.IsNullOrWhiteSpace(message.Id))
                message.Id = _context.IdGenerator.GetNewID();

            message.Timestamp = Timestamp.TimestampNow;

            if (await _context.Messenger.SendAsync(message).ConfigureAwait(false))
            {
                _sentMessages.TryAdd(message.Id, message);
                message.IsTransfered = true;
                if (!IsConfirmation(message))
                {
                    AddToConfirmationTimeoutChecking(message);
                    OnMessageSent(message);
                }
                else
                {
                    OnConfirmationSent(message);
                }
                return true;
            }
            _failedSendMessages.AddOrUpdate(message.Id, message, (_, m) => { m.Timestamp = message.Timestamp; return m; });
            OnSendingFailed(message);
            return false;
        }

        private void AddToConfirmationTimeoutChecking(IMessageModel confirmation)
        {
            _context.ConfirmationTimeoutChecker.AddToCheckingQueue(confirmation);
        }

        public BlockingCollection<IMessageModel> GetReceivedMessages() => _forGUITemporary;

        #region Event handlers implementation

        protected virtual void OnMessageSent(IMessageModel e) => MessageSent?.Invoke(this, e);

        protected virtual void OnConfirmationSent(IMessageModel e) => ConfirmationSent?.Invoke(this, e);

        protected virtual void OnMessageReceived(IMessageModel e) => MessageReceived?.Invoke(this, e);

        protected virtual void OnConfirmationReceived(IMessageModel e) => ConfirmationReceived?.Invoke(this, e);

        protected virtual void OnSendingFailed(IMessageModel e) => SendingFailed?.Invoke(this, e);

        #endregion Event handlers implementation

        #region IDispose implementation

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _forGUITemporary?.Dispose();
                    _handleNewMessagesCTS?.Dispose();
                    // Free other state (managed objects).
                }
                // Free your own state (unmanaged objects).
                // Set large fields to null.
                _disposed = true;
            }
        }

        #endregion IDispose implementation
    }
}