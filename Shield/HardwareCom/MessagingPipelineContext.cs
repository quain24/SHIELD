using Shield.HardwareCom.CommandProcessing;
using Shield.HardwareCom.Factories;
using Shield.HardwareCom.MessageProcessing;
using Shield.Helpers;
using System;

namespace Shield.HardwareCom
{
    /// <summary>
    /// Contains all necessary objects for <see cref="MessagingPipeline"/> creation and operation.
    /// </summary>
    public class MessagingPipelineContext : IMessagingPipelineContext
    {
        public MessagingPipelineContext(IMessenger messenger,
                                         ICommandIngester ingester,
                                         IIncomingMessageProcessor processor,
                                         ICompletitionTimeoutChecker completitionTimeoutChecker,
                                         IConfirmationTimeoutChecker confirmationTimeoutChecker,
                                         IIdGenerator idGenerator,
                                         IConfirmationFactory confirmationFactory)
        {
            Messenger = messenger ?? throw new ArgumentNullException(nameof(messenger));
            Ingester = ingester ?? throw new ArgumentNullException(nameof(ingester));
            Processor = processor ?? throw new ArgumentNullException(nameof(processor));
            CompletitionTimeoutChecker = completitionTimeoutChecker ?? throw new ArgumentNullException(nameof(completitionTimeoutChecker));
            ConfirmationTimeoutChecker = confirmationTimeoutChecker ?? throw new ArgumentNullException(nameof(confirmationTimeoutChecker));
            IdGenerator = idGenerator ?? throw new ArgumentNullException(nameof(idGenerator));
            ConfirmationFactory = confirmationFactory ?? throw new ArgumentNullException(nameof(confirmationFactory));
        }

        public IMessenger Messenger { get; }

        public ICommandIngester Ingester { get; }

        public IIncomingMessageProcessor Processor { get; }

        public ICompletitionTimeoutChecker CompletitionTimeoutChecker { get; }

        public IConfirmationTimeoutChecker ConfirmationTimeoutChecker { get; }

        public IIdGenerator IdGenerator { get; }

        public IConfirmationFactory ConfirmationFactory { get; }
    }
}