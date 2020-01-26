using Shield.Extensions;
using Shield.HardwareCom.Models;
using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Shield.HardwareCom
{
    /// <summary>
    /// base class for future IncomingMessageProcessor and OutgoingMessageProcessor
    /// </summary>
    public abstract class MessageProcessor : IMessageProcessor
    {
        private const int TakeTimeout = 150;
        private BlockingCollection<IMessageHWComModel> _messagesToProcess = new BlockingCollection<IMessageHWComModel>();
        private readonly BlockingCollection<IMessageHWComModel> _processedMessages = new BlockingCollection<IMessageHWComModel>();
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
        public void AddMessageToProcess(IMessageHWComModel message)
        {
            if (message is null)
                throw new ArgumentNullException(nameof(message), "MessageProcessor - AddMessageToProcess: Cannot add a NULL to processing queue");
            _messagesToProcess.Add(message);
            Console.WriteLine("message added to be processed");
        }

        /// <summary>
        /// Replace built in source collection with external collection, that for example will be updated by another object
        /// </summary>
        /// <param name="newSourceCollection">external collection</param>
        public void SwitchSourceCollection(BlockingCollection<IMessageHWComModel> newSourceCollection)
        {
            if (newSourceCollection is null)
                throw new ArgumentNullException(nameof(newSourceCollection), "New source collection cannot be NULL.");

            using (_sourceCollectionSwithLock.Read())
            {
                _messagesToProcess = newSourceCollection;
            }
        }

        /// <summary>
        /// Starts continuous message processing.
        /// Thread safe.
        /// </summary>
        public void StartProcessingMessagesContinous()
        {
            if (!_isProcessing)
            {
                lock (_processingLock)
                {
                    if (_isProcessing)
                    {
                        Console.WriteLine($@"MessageProcessor already running.");
                        return;
                    }
                    _isProcessing = true;
                }
            }

            Console.WriteLine("MessageProcessor - Continuous Message processing started");

            while (true)
            {
                _isProcessing = true;
                try
                {
                    IMessageHWComModel message = null;
                    bool wasTaken = _messagesToProcess.TryTake(out message, 150, _processingCTS.Token);

                    IMessageHWComModel processedMessage = null;

                    if (wasTaken)
                    {
                        using (_sourceCollectionSwithLock.Write())
                        {
                            TryProcess(message, out processedMessage);
                        }

                        _processedMessages.Add(processedMessage);
                        Console.WriteLine($@"MessageProcessor - Took single message ({message.Id}) to process");
                    }
                }
                catch (OperationCanceledException)
                {
                    Console.WriteLine("MessageProcessor continuous ENDED");
                    _isProcessing = false;
                    break;
                }
            }
        }

        /// <summary>
        /// Starts processing messages until there is none left in queue.
        /// thread safe
        /// </summary>
        public void StartProcessingMessages()
        {
            if (!_isProcessing)
            {
                lock (_processingLock)
                {
                    if (_isProcessing)
                        return;
                    _isProcessing = true;
                }
            }

            while (_messagesToProcess.Count > 0)
            {
                _isProcessing = true;

                try
                {
                    IMessageHWComModel processedMessage = null;
                    IMessageHWComModel message = null;

                    if (_messagesToProcess.TryTake(out message, TakeTimeout, _processingCTS.Token))
                    {
                        bool isProcessedWithoutErrors = TryProcess(message, out processedMessage);
                        _processedMessages.Add(processedMessage);
                    }
                    else
                        break;
                }
                catch
                {
                    _isProcessing = false;
                    break;
                }
            }

            _isProcessing = false;
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
        public BlockingCollection<IMessageHWComModel> GetProcessedMessages()
        {
            return _processedMessages;
        }

        /// <summary>
        /// Check given message for errors by given injected objects and return true if message was processed without errors
        /// or false if processed with some internal errors.
        /// </summary>
        /// <param name="messageToProcess">Message to be processed</param>
        /// <param name="processedMessage">If successful - processed message, otherwise processed message with set errors</param>
        /// <returns>TRUE if message was processed without errors</returns>
        public abstract bool TryProcess(IMessageHWComModel messageToProcess, out IMessageHWComModel processedMessage);
    }
}