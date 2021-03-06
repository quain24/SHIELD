﻿using Shield.HardwareCom.Helpers;
using Shield.HardwareCom.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Shield.HardwareCom
{
    /// <summary>
    /// <see cref="MessagingPipeline"/> use is to send and receive <see cref="IMessageModel"/> objects<br />
    /// Pipeline handles all needed communication through given <see cref="IMessagingPipelineContext"/><br />
    /// Pipeline will automatically try to send a <see cref="Enums.CommandType.Confirmation"/> type of <see cref="IMessageModel"/> as a response.<br /><br />
    /// Raw data decoding, error correction, message creation, timeout checking - all is done here with a <see cref="IMessagingPipelineContext"/> object.<br />
    /// <see cref="MessagingPipeline"/> is a facade.
    /// </summary>
    public class MessagingPipeline : IMessagingPipeline, IDisposable
    {
        private readonly IMessagingPipelineContext _context;
        private readonly ConcurrentDictionary<string, IMessageModel> _receivedMessages = new ConcurrentDictionary<string, IMessageModel>(StringComparer.OrdinalIgnoreCase);
        private readonly ConcurrentDictionary<string, IMessageModel> _sentMessages = new ConcurrentDictionary<string, IMessageModel>(StringComparer.OrdinalIgnoreCase);
        private readonly ConcurrentDictionary<string, IMessageModel> _failedSendMessages = new ConcurrentDictionary<string, IMessageModel>(StringComparer.OrdinalIgnoreCase);

        private CancellationTokenSource _handleNewMessagesCTS = new CancellationTokenSource();
        private bool _disposed = false;

        public MessagingPipeline(IMessagingPipelineContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _context.ConfirmationTimeoutChecker.TimeoutOccurred += TimeoutOccurredHandler;

            AssignCollections();
        }

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
                    if (IsOpen)
                        Close().ConfigureAwait(false);
                    _handleNewMessagesCTS?.Dispose();
                    // Free other state (managed objects).
                }
                // Free your own state (unmanaged objects).
                // Set large fields to null.
                _disposed = true;
            }
        }

        #endregion IDispose implementation

        #region Events

        /// <summary>
        /// A <see cref="IMessageModel"/> was sent successfully (excluding confirmation type)
        /// </summary>
        public event EventHandler<IMessageModel> MessageSent;

        /// <summary>
        /// A Confirmation type of <see cref="IMessageModel"/> was sent successfully
        /// </summary>
        public event EventHandler<IMessageModel> ConfirmationSent;

        /// <summary>
        /// <see cref="IMessageModel"/> has been received
        /// </summary>
        public event EventHandler<IMessageModel> MessageReceived;

        /// <summary>
        /// Confirmation of <see cref="IMessageModel"/> has been received
        /// </summary>
        public event EventHandler<IMessageModel> ConfirmationReceived;

        /// <summary>
        /// Sending of <see cref="IMessageModel"/> has failed - message was added to internal buffer;
        /// </summary>
        public event EventHandler<IMessageModel> SendingFailed;

        /// <summary>
        /// Confirmation Timeout has occurred - given <see cref="IMessageModel"/> was not confirmed by recipient in time
        /// </summary>
        public event EventHandler<IMessageModel> ConfirmationTimeout;

        #endregion Events

        /// <summary>
        /// Returns state of the <see cref="IMessagingPipeline"/>
        /// </summary>
        public bool IsOpen => _context?.Messenger?.IsOpen ?? false;

        private void AssignCollections()
        {
            _context.Ingester.SwitchSourceCollectionTo(_context.Messenger.GetReceivedCommands());
            _context.Processor.SwitchSourceCollectionTo(_context.Ingester.GetReceivedMessages());
        }

        /// <summary>
        /// Opens pipeline to receive and send messages.
        /// </summary>
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

        /// <summary>
        /// Close pipeline
        /// </summary>
        public async Task Close()
        {
            CancelHandleIncoming();
            List<Task> tasksToCancell = new List<Task>();
            if (_context.ConfirmationTimeoutChecker.IsWorking) tasksToCancell.Add(Task.Run(() => _context.ConfirmationTimeoutChecker.StopCheckingUnconfirmedMessages()));
            if (_context.CompletitionTimeoutChecker.IsWorking) tasksToCancell.Add(Task.Run(() => _context.CompletitionTimeoutChecker.StopTimeoutCheck()));
            if (_context.Processor.IsProcessingMessages) tasksToCancell.Add(Task.Run(() => _context.Processor.StopProcessingMessages()));
            if (_context.Ingester.IsProcessingCommands) tasksToCancell.Add(Task.Run(() => _context.Ingester.StopProcessingCommands()));
            if (_context.Messenger.IsReceiving) tasksToCancell.Add(Task.Run(() => _context.Messenger.StopReceiving()));
            if (_context.Messenger.IsOpen) tasksToCancell.Add(Task.Run(() => _context.Messenger.Close()));

            await Task.WhenAll(tasksToCancell).ConfigureAwait(false);
        }

        private async Task HandleIncoming()
        {
            IMessageModel receivedMessage;
            while (!_handleNewMessagesCTS.IsCancellationRequested)
            {
                try
                {
                    receivedMessage = GetNextReceivedMessage(_handleNewMessagesCTS);
                }
                catch (OperationCanceledException)
                {
                    break;
                }

                receivedMessage.IsTransfered = true;
                if (IsConfirmation(receivedMessage))
                    HandleReceivedConfirmation(receivedMessage);
                else
                    await HandleReceivedMessage(receivedMessage).ConfigureAwait(false);
            }
        }

        private void CancelHandleIncoming()
        {
            _handleNewMessagesCTS.Cancel();
            _handleNewMessagesCTS = new CancellationTokenSource();
        }

        private IMessageModel GetNextReceivedMessage(CancellationTokenSource cts) =>
            _context.Processor.GetProcessedMessages().Take(cts.Token);

        private static bool IsConfirmation(IMessageModel message) => message?.Type == Enums.MessageType.Confirmation;

        private void HandleReceivedConfirmation(IMessageModel confirmation)
        {
            _context.ConfirmationTimeoutChecker.AddConfirmation(confirmation);
            _receivedMessages.TryAdd(confirmation.Id, confirmation);
            OnConfirmationReceived(confirmation);
        }

        private async Task HandleReceivedMessage(IMessageModel message)
        {
            _receivedMessages.TryAdd(message.Id, message);
            OnMessageReceived(message);
            await SendConfirmationOfAsync(message).ConfigureAwait(false);
        }

        private async Task SendConfirmationOfAsync(IMessageModel message)
        {
            IMessageModel confirmation = _context.ConfirmationFactory.GenerateConfirmationOf(message);
            if (await SendAsync(confirmation).ConfigureAwait(false))
                _sentMessages.TryAdd(confirmation.Id, confirmation);
        }

        /// <summary>
        /// Sends a <see cref="IMessageModel"/> by pipeline.
        /// </summary>
        /// <param name="message"><see cref="IMessageModel"/> to be sent</param>
        /// <returns>True if <see cref="IMessageModel"/> sent successfully</returns>
        public async Task<bool> SendAsync(IMessageModel message)
        {
            message.Timestamp = Timestamp.TimestampNow;

            if (!CanSend(message))
            {
                SendFailureHandler(message);
                return false;
            }

            AssignIdIfNeeded(message);

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
            SendFailureHandler(message);
            return false;
        }

        private bool CanSend(IMessageModel message) => IsOpen && message != null;

        private void SendFailureHandler(IMessageModel message)
        {
            message.IsTransfered = false;
            PushToFailedSentMessages(message);
            OnSendingFailed(message);
        }

        private void PushToFailedSentMessages(IMessageModel message)
        {
            _failedSendMessages.AddOrUpdate(message.Id, message, (_, m) => { m.Timestamp = message.Timestamp; return m; });
        }

        private void AssignIdIfNeeded(IMessageModel message)
        {
            if (string.IsNullOrWhiteSpace(message.Id))
                message.Id = _context.IdGenerator.GetNewID();
        }

        private void AddToConfirmationTimeoutChecking(IMessageModel confirmation)
        {
            _context.ConfirmationTimeoutChecker.AddToCheckingQueue(confirmation);
        }

        public async Task RetryFailedSends()
        {
            foreach (var kvp in _failedSendMessages)
            {
                IMessageModel message = kvp.Value;
                if (CanSend(message) && await SendAsync(message).ConfigureAwait(false))
                    _failedSendMessages.TryRemove(kvp.Key, out _);
            }
        }

        public ConcurrentDictionary<string, IMessageModel> GetReceivedMessages() => _receivedMessages;

        public ConcurrentDictionary<string, IMessageModel> GetSentMessages() => _sentMessages;

        public ConcurrentDictionary<string, IMessageModel> GetFailedSendMessages() => _failedSendMessages;

        #region Event handlers implementation

        protected virtual void OnMessageSent(IMessageModel e) => MessageSent?.Invoke(this, e);

        protected virtual void OnConfirmationSent(IMessageModel e) => ConfirmationSent?.Invoke(this, e);

        protected virtual void OnMessageReceived(IMessageModel e) => MessageReceived?.Invoke(this, e);

        protected virtual void OnConfirmationReceived(IMessageModel e) => ConfirmationReceived?.Invoke(this, e);

        protected virtual void OnSendingFailed(IMessageModel e) => SendingFailed?.Invoke(this, e);

        private void TimeoutOccurredHandler(object sender, IMessageModel e) => OnConfirmationTimeout(e);

        protected virtual void OnConfirmationTimeout(IMessageModel e) => ConfirmationTimeout?.Invoke(this, e);

        #endregion Event handlers implementation
    }
}