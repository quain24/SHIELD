using Shield.CommonInterfaces;
using Shield.HardwareCom.MessageProcessing;
using Shield.Helpers;
using System;

namespace Shield.HardwareCom.Factories
{
    public class MessagePipelineContextFactory : IMessagePipelineContextFactory
    {
        private readonly ITimeoutCheckFactory _timeoutCheckFactory;
        private readonly IMessengerFactory _messengerFactory;
        private readonly ICommandIngesterFactory _ingesterFactory;
        private readonly Func<IIncomingMessageProcessor> _processorFactory;
        private readonly IConfirmationTimeoutCheckerFactory _confirmationTimeoutCheckerFactory;
        private readonly ICompletitionTimeoutCheckerFactory _completitionTimeoutCheckerFactory;
        private readonly Func<IConfirmationFactory> _confirmationFactory;
        private readonly Func<IIdGenerator> _idGeneratorAutoFac;

        public MessagePipelineContextFactory(ITimeoutCheckFactory timeoutCheckFactory,
                                             IMessengerFactory messengerFactory,
                                             ICommandIngesterFactory ingesterFactory,
                                             Func<IIncomingMessageProcessor> processorFactory,
                                             IConfirmationTimeoutCheckerFactory confirmationTimeoutCheckerFactory,
                                             ICompletitionTimeoutCheckerFactory completitionTimeoutCheckerFactory,
                                             Func<IConfirmationFactory> confirmationFactory,
                                             Func<IIdGenerator> idGeneratorAutoFac)
        {
            _timeoutCheckFactory = timeoutCheckFactory ?? throw new ArgumentNullException(nameof(timeoutCheckFactory));
            _messengerFactory = messengerFactory ?? throw new ArgumentNullException(nameof(messengerFactory));
            _ingesterFactory = ingesterFactory ?? throw new ArgumentNullException(nameof(ingesterFactory));
            _processorFactory = processorFactory ?? throw new ArgumentNullException(nameof(processorFactory));
            _confirmationTimeoutCheckerFactory = confirmationTimeoutCheckerFactory ?? throw new ArgumentNullException(nameof(confirmationTimeoutCheckerFactory));
            _completitionTimeoutCheckerFactory = completitionTimeoutCheckerFactory ?? throw new ArgumentNullException(nameof(completitionTimeoutCheckerFactory));
            _confirmationFactory = confirmationFactory ?? throw new ArgumentNullException(nameof(confirmationFactory));
            _idGeneratorAutoFac = idGeneratorAutoFac ?? throw new ArgumentNullException(nameof(idGeneratorAutoFac));
        }

        public IMessagePipelineContext GetContextFor(ICommunicationDevice device)
        {
            IIdGenerator idGenerator = _idGeneratorAutoFac();
            ICommandIngesterAlt ingester = _ingesterFactory.GetIngesterUsing(idGenerator);
            IIncomingMessageProcessor processor = _processorFactory();
            IConfirmationTimeoutChecker confirmationTimeoutChecker = _confirmationTimeoutCheckerFactory.GetCheckerUsing(device.ConfirmationTimeout);
            ITimeoutCheck completitionTimeoutCheck = _timeoutCheckFactory.GetTimeoutCheckWithTimeoutSetTo(device.CompletitionTimeout);
            ICompletitionTimeoutChecker completitionTimeoutChecker = _completitionTimeoutCheckerFactory.CreateCompletitionTimoutCheckerUsing(ingester, completitionTimeoutCheck);
            IMessenger messenger = _messengerFactory.CreateMessangerUsing(device);
            IConfirmationFactory confirmationFactory = _confirmationFactory();

            return new MessagePipelineContext(messenger, ingester, processor, completitionTimeoutChecker, confirmationTimeoutChecker, idGenerator, confirmationFactory);
        }
    }
}