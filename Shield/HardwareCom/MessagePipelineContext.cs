﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shield.HardwareCom.Factories;
using Shield.HardwareCom.MessageProcessing;
using Shield.Helpers;

namespace Shield.HardwareCom
{
    public class MessagePipelineContext : IMessagePipelineContext
    {
        private readonly IMessenger _messenger;
        private readonly ICommandIngesterAlt _ingester;
        private readonly IIncomingMessageProcessor _processor;
        private readonly ICompletitionTimeoutChecker _completitionTimeoutChecker;
        private readonly IConfirmationTimeoutChecker _confirmationTimeoutChecker;
        private readonly IIdGenerator _idGenerator;
        private readonly IConfirmationFactory _confirmationFactory;

        public MessagePipelineContext(IMessenger messenger,
                                      ICommandIngesterAlt ingester,
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

        public ICommandIngesterAlt Ingester => _ingester;

        public IIncomingMessageProcessor Processor => _processor;

        public ICompletitionTimeoutChecker CompletitionTimeoutChecker => _completitionTimeoutChecker;

        public IConfirmationTimeoutChecker ConfirmationTimeoutChecker => _confirmationTimeoutChecker;

        public IIdGenerator IdGenerator => _idGenerator;

        public IConfirmationFactory ConfirmationFactory => _confirmationFactory;
    }
}
