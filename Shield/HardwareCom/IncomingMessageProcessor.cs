using Shield.Enums;
using Shield.HardwareCom.MessageProcessing;
using Shield.HardwareCom.Models;
using System;

namespace Shield.HardwareCom
{
    /// <summary>
    /// Gets incoming messages and checks them for errors. Provides two thread safe collections (clean and 'with errors' messages)
    /// for further processing / handling by another class. Is derived from <see cref="MessageProcessor"/> and implements
    /// <seealso cref="IMessageProcessor"/>
    /// </summary>
    public class IncomingMessageProcessor : MessageProcessor
    {
        private readonly IDecoding _decoding;
        private readonly IPattern _pattern;
        private readonly ITypeDetector _typeDetector;

        public IncomingMessageProcessor(IDecoding decoding,
                                        IPattern pattern,
                                        ITypeDetector typeDetector)
        {
            _decoding = decoding;
            _pattern = pattern;
            _typeDetector = typeDetector;
        }

        public override bool TryProcess(IMessageHWComModel messageToProcess, out IMessageHWComModel processedMessage)
        {
            if (messageToProcess is null)
                throw new ArgumentNullException(nameof(messageToProcess), "IncomingMessageProcessor - TryProcess: Cannot process NULL.");
            
            processedMessage = messageToProcess;

            // check for decoding errors
            messageToProcess.Errors = messageToProcess.Errors | _decoding.Check(messageToProcess);

            // check for errors in message pattern
            if (_pattern.IsCorrect(messageToProcess) == false)
                messageToProcess.Errors = messageToProcess.Errors | Errors.BadMessagePattern;

            // try to detect message type
            messageToProcess.Type = _typeDetector.DetectTypeOf(messageToProcess);
            if (messageToProcess.Type == MessageType.Unknown)
                messageToProcess.Errors = messageToProcess.Errors | Errors.UndeterminedType;

            Console.WriteLine($@"IncomingMessageProcessor - TryProcess message running with id:{messageToProcess.Id}");

            return messageToProcess.Errors == Errors.None ? true : false;
        }
    }
}