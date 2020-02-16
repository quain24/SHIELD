using Shield.Extensions;
using Shield.HardwareCom.Models;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Shield.HardwareCom
{
    /// <summary>
    /// base class for future IncomingMessageProcessor and OutgoingMessageProcessor
    /// </summary>
    public abstract class MessageProcessor : IMessageProcessor
    {
        private const int TakeTimeout = 150;
        private BlockingCollection<IMessageModel> _messagesToProcess = new BlockingCollection<IMessageModel>();
        private readonly BlockingCollection<IMessageModel> _processedMessages = new BlockingCollection<IMessageModel>();
        private CancellationTokenSource _processingCTS = new CancellationTokenSource();
        private bool _isProcessing = false;
        private object _processingLock = new object();
        private ReaderWriterLockSlim _sourceCollectionSwithLock = new ReaderWriterLockSlim();

        /// <summary>
        /// True if currently processing messages or actively awaiting new ones to be processed
        /// </summary>
        public bool IsProcessingMessages
        {
            get => _isProcessing;
        }

        /// <summary>
        /// Add a message to be processed (thread safe)
        /// </summary>
        /// <param name="message"></param>
        public void AddMessageToProcess(IMessageModel message)
        {
            if (message is null)
                throw new ArgumentNullException(nameof(message));
            _messagesToProcess.Add(message);
            Debug.WriteLine($@"message {message.Id} added to be processed");
        }

        /// <summary>
        /// Replace built in source collection with external collection, that for example will be updated by another object
        /// </summary>
        /// <param name="newSourceCollection">external collection</param>
        public void SwitchSourceCollection(BlockingCollection<IMessageModel> newSourceCollection)
        {
            if (newSourceCollection is null)
                throw new ArgumentNullException(nameof(newSourceCollection));

            using (_sourceCollectionSwithLock.Write())
                _messagesToProcess = newSourceCollection;
        }

        /// <summary>
        /// Starts continuous message processing.
        /// Thread safe.
        /// </summary>
        public void StartProcessingMessagesContinous()
        {
            try
            {
                if (!CanStartProcessingMessages())
                    return;

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
                if (!CanStartProcessingMessages())
                    return;

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

            IMessageModel message;
            IMessageModel processedMessage;
            bool wasTaken = false;

            // Taking element from thread safe collection - no need to lock. Locking only when collection is switched to other one
            using (_sourceCollectionSwithLock.Read())
                wasTaken = _messagesToProcess.TryTake(out message, TakeTimeout, _processingCTS.Token);

            if (wasTaken)
            {
                TryProcess(message, out processedMessage);
                _processedMessages.Add(processedMessage);
                Debug.WriteLine($@"MessageProcessor - Took message ({message.Id}) and added it to output queue.");
                _processingCTS.Token.ThrowIfCancellationRequested();
            }
        }

        private bool IsMessageProcessingCorrectlyCancelled(Exception e)
        {
            if (e is TaskCanceledException || e is OperationCanceledException)
            {
                lock (_processingLock)
                    _isProcessing = false;
                return true;
            }
            return false;
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
        public BlockingCollection<IMessageModel> GetProcessedMessages()
        {
            return _processedMessages;
        }

        /// <summary>
        /// Check given message for errors by given injected objects and return true if message was processed without errors
        /// or false if processed with some internal errors. Sets according error flags.
        /// </summary>
        /// <param name="messageToProcess">Message to be processed</param>
        /// <param name="processedMessage">If successful - processed message, otherwise processed message with set errors</param>
        /// <returns>TRUE if message was processed without errors</returns>
        public abstract bool TryProcess(IMessageModel messageToProcess, out IMessageModel processedMessage);
    }
}