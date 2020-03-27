using Shield.CommonInterfaces;
using Shield.HardwareCom.CommandProcessing;
using Shield.HardwareCom.MessageProcessing;
using Shield.Helpers;
using System;

namespace Shield.HardwareCom.Factories
{
    public class MessengingPipelineContextFactory : IMessengingPipelineContextFactory
    {
        private readonly ITimeoutCheckFactory _timeoutCheckFactory;
        private readonly IMessengerFactory _messengerFactory;
        private readonly ICommandIngesterFactory _ingesterFactory;
        private readonly Func<IIncomingMessageProcessor> _processorFactory;
        private readonly IConfirmationTimeoutCheckerFactory _confirmationTimeoutCheckerFactory;
        private readonly ICompletitionTimeoutCheckerFactory _completitionTimeoutCheckerFactory;
        private readonly Func<IConfirmationFactory> _confirmationFactory;
        private readonly Func<IIdGenerator> _idGeneratorAutoFac;

        public MessengingPipelineContextFactory(ITimeoutCheckFactory timeoutCheckFactory,
                                             IMessengerFactory messengerFactory,
                                             ICommandIngesterFactory ingesterFactory,
                                             IConfirmationTimeoutCheckerFactory confirmationTimeoutCheckerFactory,
                                             ICompletitionTimeoutCheckerFactory completitionTimeoutCheckerFactory,
                                             Func<IIncomingMessageProcessor> processorFactory,
                                             Func<IConfirmationFactory> confirmationFactory,
                                             Func<IIdGenerator> idGeneratorFactory)
        {
            _timeoutCheckFactory = timeoutCheckFactory ?? throw new ArgumentNullException(nameof(timeoutCheckFactory));
            _messengerFactory = messengerFactory ?? throw new ArgumentNullException(nameof(messengerFactory));
            _ingesterFactory = ingesterFactory ?? throw new ArgumentNullException(nameof(ingesterFactory));
            _confirmationTimeoutCheckerFactory = confirmationTimeoutCheckerFactory ?? throw new ArgumentNullException(nameof(confirmationTimeoutCheckerFactory));
            _completitionTimeoutCheckerFactory = completitionTimeoutCheckerFactory ?? throw new ArgumentNullException(nameof(completitionTimeoutCheckerFactory));
            _processorFactory = processorFactory ?? throw new ArgumentNullException(nameof(processorFactory));
            _confirmationFactory = confirmationFactory ?? throw new ArgumentNullException(nameof(confirmationFactory));
            _idGeneratorAutoFac = idGeneratorFactory ?? throw new ArgumentNullException(nameof(idGeneratorFactory));
        }

        public IMessengingPipelineContext GetContextFor(ICommunicationDevice device)
        {
            IIdGenerator idGenerator = _idGeneratorAutoFac();
            ICommandIngester ingester = _ingesterFactory.GetIngesterUsing(idGenerator);
            IIncomingMessageProcessor processor = _processorFactory();
            IConfirmationTimeoutChecker confirmationTimeoutChecker = _confirmationTimeoutCheckerFactory.GetCheckerUsing(device.ConfirmationTimeout);
            ITimeoutCheck completitionTimeoutCheck = _timeoutCheckFactory.GetTimeoutCheckWithTimeoutSetTo(device.CompletitionTimeout);
            ICompletitionTimeoutChecker completitionTimeoutChecker = _completitionTimeoutCheckerFactory.GetCheckerUsing(ingester, completitionTimeoutCheck);
            IMessenger messenger = _messengerFactory.CreateMessangerUsing(device);
            IConfirmationFactory confirmationFactory = _confirmationFactory();

            return new MessengingPipelineContext(messenger, ingester, processor, completitionTimeoutChecker, confirmationTimeoutChecker, idGenerator, confirmationFactory);
        }
    }
}