using Shield.Enums;
using Shield.HardwareCom.MessageProcessing;
using Shield.HardwareCom.Models;
using System;
using System.Diagnostics;

namespace Shield.HardwareCom
{
    /// <summary>
    /// Gets incoming messages and checks them for errors using one or more <see cref="IMessageAnalyzer"/> objects. Provides two thread safe collections (clean and 'with errors' messages)
    /// for further processing / handling by another class. Is derived from <see cref="MessageProcessor"/> and implements
    /// <seealso cref="IMessageProcessor"/> by inheritance.
    /// </summary>
    public class IncomingMessageProcessor : MessageProcessor
    {
        private readonly IMessageAnalyzer[] _analyzers;

        public IncomingMessageProcessor(IMessageAnalyzer[] analyzers) => _analyzers = analyzers;

        public override bool TryProcess(IMessageModel messageToProcess, out IMessageModel processedMessage)
        {
            processedMessage = messageToProcess ?? throw new ArgumentNullException(nameof(messageToProcess));

            RunAnalyzersOn(processedMessage);

            Debug.WriteLine($@"IncomingMessageProcessor - TryProcess message running with id:{messageToProcess.Id}");

            return messageToProcess.Errors == Errors.None;
        }

        private void RunAnalyzersOn(IMessageModel message)
        {
            if (_analyzers?.Length > 0)
                for (int i = 0; i < _analyzers.Length; i++)
                    _analyzers[i]?.CheckAndSetFlagsIn(message);
        }
    }
}