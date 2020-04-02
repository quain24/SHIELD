using Shield.Extensions;
using Shield.HardwareCom.Enums;
using Shield.HardwareCom.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Shield.HardwareCom.MessageProcessing
{
    /// <summary>
    /// Gets incoming messages and checks them for errors using one or more <see cref="IMessageAnalyzer"/> objects. Provides two thread safe collections (clean and 'with errors' messages)
    /// for further processing / handling by another class.
    /// </summary>
    public class IncomingMessageProcessor : IIncomingMessageProcessor
    {
        private readonly IList<IMessageAnalyzer> _analyzers;
        private const int TakeTimeout = 150;

        private BlockingCollection<IMessageModel> _messagesToProcess = new BlockingCollection<IMessageModel>();
        private readonly BlockingCollection<IMessageModel> _processedMessages = new BlockingCollection<IMessageModel>();

        private CancellationTokenSource _processingCTS = new CancellationTokenSource();
        private bool _isProcessing = false;
        private object _processingLock = new object();

        public IncomingMessageProcessor(IList<IMessageAnalyzer> analyzers) =>
            _analyzers = analyzers ?? throw new ArgumentNullException(nameof(analyzers));

        /// <summary>
        /// True if currently processing messages or actively awaiting new ones to be processed
        /// </summary>
        public bool IsProcessingMessages => _isProcessing;

        /// <summary>
        /// Add a message to be processed (thread safe)
        /// </summary>
        /// <param name="message"></param>
        public void AddMessageToProcess(IMessageModel message)
        {
            _ = message ?? throw new ArgumentNullException(nameof(message));
            _messagesToProcess.Add(message);
        }

        /// <summary>
        /// Replace built in source collection with external collection, that for example will be updated by another object
        /// </summary>
        /// <param name="newSourceCollection">external collection</param>
        public void SwitchSourceCollectionTo(BlockingCollection<IMessageModel> newSourceCollection) =>
                _messagesToProcess = newSourceCollection ?? throw new ArgumentNullException(nameof(newSourceCollection));

        /// <summary>
        /// Starts continuous message processing.
        /// Thread safe.
        /// </summary>
        public void StartProcessingMessagesContinous()
        {
            try
            {
                if (CanStartProcessingMessages())
                    while (true)
                        TryProcessNextMessage();
            }
            catch (Exception e)
            {
                if (!IsMessageProcessingCorrectlyCancelled(e))
                    throw;
            }
        }

        /// <summary>
        /// Starts processing messages until there is none left in queue.
        /// thread safe
        /// </summary>
        public void StartProcessingMessages()
        {
            try
            {
                if (CanStartProcessingMessages())
                    while (_messagesToProcess.Count > 0)
                        TryProcessNextMessage();
            }
            catch (Exception e)
            {
                if (!IsMessageProcessingCorrectlyCancelled(e))
                    throw;
            }
        }

        private bool CanStartProcessingMessages()
        {
            lock (_processingLock)
            {
                if (_isProcessing)
                    return false;
                return _isProcessing = true;
            }
        }

        private void TryProcessNextMessage()
        {
            _processingCTS.Token.ThrowIfCancellationRequested();

            bool wasTaken = _messagesToProcess.TryTake(out IMessageModel message, TakeTimeout, _processingCTS.Token);

            if (wasTaken)
            {
                TryProcess(message, out IMessageModel processedMessage);
                _processedMessages.Add(processedMessage);
                _processingCTS.Token.ThrowIfCancellationRequested();
            }
        }

        public bool TryProcess(IMessageModel messageToProcess, out IMessageModel processedMessage)
        {
            processedMessage = messageToProcess ?? throw new ArgumentNullException(nameof(messageToProcess));

            RunAnalyzersOn(processedMessage);

            return messageToProcess.Errors == Errors.None;
        }

        private void RunAnalyzersOn(IMessageModel message)
        {
            if (_analyzers.Count > 0)
                for (int i = 0; i < _analyzers.Count; i++)
                    _analyzers[i]?.CheckAndSetFlagsIn(message);
        }

        private bool IsMessageProcessingCorrectlyCancelled(Exception e)
        {
            lock (_processingLock)
                _isProcessing = false;
            return e is TaskCanceledException || e is OperationCanceledException;
        }

        /// <summary>
        /// Stops continuous message processing
        /// </summary>
        public void StopProcessingMessages()
        {
            _processingCTS.Cancel();
            _processingCTS = new CancellationTokenSource();
        }

        /// <summary>
        /// Gets thread safe collection that contains processed messages.
        /// </summary>
        /// <returns>Collection of processed messages</returns>
        public BlockingCollection<IMessageModel> GetProcessedMessages() =>
            _processedMessages;
    }
}