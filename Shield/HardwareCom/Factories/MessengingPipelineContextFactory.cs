using Shield.CommonInterfaces;
using Shield.HardwareCom.CommandProcessing;
using Shield.HardwareCom.MessageProcessing;
using Shield.Helpers;
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

        public MessengingPipelineContextFactory(IMessengerFactory messengerFactory,
                                                ICommandIngesterFactory ingesterFactory,
                                                Func<ITimeout, IConfirmationTimeoutChecker> confirmationTImeoutChecker,
                                                Func<ICommandIngester, ITimeout, ICompletitionTimeoutChecker> completitionCheckFactory,
                                                Func<IIncomingMessageProcessor> processorFactory,
                                                Func<IConfirmationFactory> confirmationFactory,
                                                Func<IIdGenerator> idGeneratorFactory,
                                                ITimeoutFactory timeoutFactory)
        {
            _messengerFactory = messengerFactory ?? throw new ArgumentNullException(nameof(messengerFactory));
            _ingesterFactory = ingesterFactory ?? throw new ArgumentNullException(nameof(ingesterFactory));
            _confirmationTImeoutChecker = confirmationTImeoutChecker ?? throw new ArgumentNullException(nameof(confirmationTImeoutChecker));
            _completitionCheckFactory = completitionCheckFactory ?? throw new ArgumentNullException(nameof(completitionCheckFactory));
            _processorFactory = processorFactory ?? throw new ArgumentNullException(nameof(processorFactory));
            _confirmationFactory = confirmationFactory ?? throw new ArgumentNullException(nameof(confirmationFactory));
            _idGeneratorFactory = idGeneratorFactory ?? throw new ArgumentNullException(nameof(idGeneratorFactory));
            _timeoutFactory = timeoutFactory ?? throw new ArgumentNullException(nameof(timeoutFactory));
        }

        // TODO clean this up

        public IMessengingPipelineContext GetContextFor(ICommunicationDevice device)
        {
            _ = device ?? throw new ArgumentNullException(nameof(device));

            IMessenger messenger = _messengerFactory.CreateMessangerUsing(device);

            IIdGenerator idGenerator = _idGeneratorFactory();
            ICommandIngester ingester = _ingesterFactory.GetIngesterUsing(idGenerator);

            IIncomingMessageProcessor processor = _processorFactory();

            ITimeout confirmationTimoutCheck = _timeoutFactory.CreateTimeoutWith(device.ConfirmationTimeout);
            IConfirmationTimeoutChecker confirmationTimeoutChecker = _confirmationTImeoutChecker(confirmationTimoutCheck);

            ITimeout completitionTimeoutCheck = _timeoutFactory.CreateTimeoutWith(device.CompletitionTimeout);
            ICompletitionTimeoutChecker completitionTimeoutChecker = _completitionCheckFactory(ingester, completitionTimeoutCheck);

            IConfirmationFactory confirmationFactory = _confirmationFactory();

            return new MessengingPipelineContext(messenger, ingester, processor, completitionTimeoutChecker, confirmationTimeoutChecker, idGenerator, confirmationFactory);
        }
    }
}