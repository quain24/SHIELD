using Shield.CommonInterfaces;
using Shield.HardwareCom.CommandProcessing;
using Shield.HardwareCom.MessageProcessing;
using Shield.HardwareCom.Helpers;
using System;

namespace Shield.HardwareCom.Factories
{
    public class MessengingPipelineContextFactory : IMessengingPipelineContextFactory
    {
        private readonly IMessengerFactory _messengerFactory;
        private readonly ICommandIngesterFactory _ingesterFactory;
        private readonly Func<IIncomingMessageProcessor> _processorFactory;
        private readonly Func<ITimeout, IConfirmationTimeoutChecker> _confirmationTImeoutChecker;
        private readonly Func<ICommandIngester, ITimeout, ICompletitionTimeoutChecker> _completitionCheckFactory;
        private readonly Func<IConfirmationFactory> _confirmationFactory;
        private readonly Func<IIdGenerator> _idGeneratorFactory;
        private readonly ITimeoutFactory _timeoutFactory;

        private readonly Func<IMessenger,
                              ICommandIngester,
                              IIncomingMessageProcessor,
                              ICompletitionTimeoutChecker,
                              IConfirmationTimeoutChecker,
                              IIdGenerator,
                              IConfirmationFactory,
                              IMessagingPipelineContext> _messengingPipelineContextAF;

        private IIdGenerator _idGenerator = null;
        private ICommandIngester _ingester = null;

        public MessengingPipelineContextFactory(IMessengerFactory messengerFactory,
                                                ICommandIngesterFactory ingesterFactory,
                                                Func<ITimeout, IConfirmationTimeoutChecker> confirmationTImeoutChecker,
                                                Func<ICommandIngester, ITimeout, ICompletitionTimeoutChecker> completitionCheckFactory,
                                                Func<IIncomingMessageProcessor> processorFactory,
                                                Func<IConfirmationFactory> confirmationFactory,
                                                Func<IIdGenerator> idGeneratorFactory,
                                                ITimeoutFactory timeoutFactory,
                                                Func<IMessenger,
                                                     ICommandIngester,
                                                     IIncomingMessageProcessor,
                                                     ICompletitionTimeoutChecker,
                                                     IConfirmationTimeoutChecker,
                                                     IIdGenerator,
                                                     IConfirmationFactory,
                                                     IMessagingPipelineContext> messengingPipelineContextAF)
        {
            _messengerFactory = messengerFactory ?? throw new ArgumentNullException(nameof(messengerFactory));
            _ingesterFactory = ingesterFactory ?? throw new ArgumentNullException(nameof(ingesterFactory));
            _confirmationTImeoutChecker = confirmationTImeoutChecker ?? throw new ArgumentNullException(nameof(confirmationTImeoutChecker));
            _completitionCheckFactory = completitionCheckFactory ?? throw new ArgumentNullException(nameof(completitionCheckFactory));
            _processorFactory = processorFactory ?? throw new ArgumentNullException(nameof(processorFactory));
            _confirmationFactory = confirmationFactory ?? throw new ArgumentNullException(nameof(confirmationFactory));
            _idGeneratorFactory = idGeneratorFactory ?? throw new ArgumentNullException(nameof(idGeneratorFactory));
            _timeoutFactory = timeoutFactory ?? throw new ArgumentNullException(nameof(timeoutFactory));
            _messengingPipelineContextAF = messengingPipelineContextAF ?? throw new ArgumentNullException(nameof(messengingPipelineContextAF));
        }

        public IMessagingPipelineContext GetContextFor(ICommunicationDevice device)
        {
            _ = device ?? throw new ArgumentNullException(nameof(device));

            return _messengingPipelineContextAF(
                Messenger(device),
                Ingester(),
                IncomingMessageProcessor(),
                CompletitionTimeoutChecker(device),
                ConfirmationTimeoutChecker(device),
                IdGenerator(),
                ConfirmationFactory());
        }

        private IMessenger Messenger(ICommunicationDevice device) => _messengerFactory.CreateMessangerUsing(device);

        private IIdGenerator IdGenerator() => _idGenerator ?? (_idGenerator = _idGeneratorFactory());

        private ICommandIngester Ingester() => _ingester ?? (_ingester = _ingesterFactory.GetIngesterUsing(IdGenerator()));

        private IIncomingMessageProcessor IncomingMessageProcessor() => _processorFactory();

        private IConfirmationTimeoutChecker ConfirmationTimeoutChecker(ICommunicationDevice device) =>
            _confirmationTImeoutChecker(_timeoutFactory.CreateTimeoutWith(device.ConfirmationTimeout));

        private ICompletitionTimeoutChecker CompletitionTimeoutChecker(ICommunicationDevice device) =>
            _completitionCheckFactory(Ingester(), _timeoutFactory.CreateTimeoutWith(device.CompletitionTimeout));

        private IConfirmationFactory ConfirmationFactory() => _confirmationFactory();
    }
}