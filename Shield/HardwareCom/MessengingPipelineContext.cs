using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shield.HardwareCom.CommandProcessing;
using Shield.HardwareCom.Factories;
using Shield.HardwareCom.MessageProcessing;
using Shield.Helpers;

namespace Shield.HardwareCom
{
    /// <summary>
    /// Contains all necessary objects for <see cref="MessengingPipeline"/> creation and operation.
    /// </summary>
    public class MessengingPipelineContext : IMessengingPipelineContext
    {
        private readonly IMessenger _messenger;
        private readonly ICommandIngester _ingester;
        private readonly IIncomingMessageProcessor _processor;
        private readonly ICompletitionTimeoutChecker _completitionTimeoutChecker;
        private readonly IConfirmationTimeoutChecker _confirmationTimeoutChecker;
        private readonly IIdGenerator _idGenerator;
        private readonly IConfirmationFactory _confirmationFactory;
                
        public MessengingPipelineContext(IMessenger messenger,
                                         ICommandIngester ingester,
                                         IIncomingMessageProcessor processor,
                                         ICompletitionTimeoutChecker completitionTimeoutChecker,
                                         IConfirmationTimeoutChecker confirmationTimeoutChecker,
                                         IIdGenerator idGenerator,
                                         IConfirmationFactory confirmationFactory)
        {
            _messenger = messenger ?? throw new ArgumentNullException(nameof(messenger));
            _ingester = ingester ?? throw new ArgumentNullException(nameof(ingester));
            _processor = processor ?? throw new ArgumentNullException(nameof(processor));
            _completitionTimeoutChecker = completitionTimeoutChecker ?? throw new ArgumentNullException(nameof(completitionTimeoutChecker));
            _confirmationTimeoutChecker = confirmationTimeoutChecker ?? throw new ArgumentNullException(nameof(confirmationTimeoutChecker));
            _idGenerator = idGenerator ?? throw new ArgumentNullException(nameof(idGenerator));
            _confirmationFactory = confirmationFactory ?? throw new ArgumentNullException(nameof(confirmationFactory));
        }

        public IMessenger Messenger => _messenger;

        public ICommandIngester Ingester => _ingester;

        public IIncomingMessageProcessor Processor => _processor;

        public ICompletitionTimeoutChecker CompletitionTimeoutChecker => _completitionTimeoutChecker;

        public IConfirmationTimeoutChecker ConfirmationTimeoutChecker => _confirmationTimeoutChecker;

        public IIdGenerator IdGenerator => _idGenerator;

        public IConfirmationFactory ConfirmationFactory => _confirmationFactory;
    }
}
