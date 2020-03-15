using Shield.CommonInterfaces;
using Shield.HardwareCom.MessageProcessing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shield.HardwareCom.Factories
{
    public class MessengingPipelineFactory : IMessengingPipelineFactory
    {
        private readonly IMessengerFactory _messengerFactory;
        private readonly ICommandIngesterFactory _ingesterFactory;
        private readonly Func<IIncomingMessageProcessor> _processorFactory;
        private readonly IConfirmationTimeoutCheckerFactory _confirmationTimeoutCheckerFactory;
        private readonly Func<IConfirmationFactory> _confirmationFactory;

        public MessengingPipelineFactory(IMessengerFactory messengerFactory,
                                         ICommandIngesterFactory ingesterFactory,
                                         Func<IIncomingMessageProcessor> processorFactory,
                                         IConfirmationTimeoutCheckerFactory confirmationTimeoutCheckerFactory,
                                         Func<IConfirmationFactory> confirmationFactory)
        {
            _messengerFactory = messengerFactory ?? throw new ArgumentNullException(nameof(messengerFactory));
            _ingesterFactory = ingesterFactory ?? throw new ArgumentNullException(nameof(ingesterFactory));
            _processorFactory = processorFactory ?? throw new ArgumentNullException(nameof(processorFactory));
            _confirmationTimeoutCheckerFactory = confirmationTimeoutCheckerFactory ?? throw new ArgumentNullException(nameof(confirmationTimeoutCheckerFactory));
            _confirmationFactory = confirmationFactory ?? throw new ArgumentNullException(nameof(confirmationFactory));
        }

        /// <summary>
        /// Creates a <see cref="MessengingPipeline"/> for given <see cref="ICommunicationDevice"/> 
        /// </summary>
        /// <param name="device">Device used to send and receive data by pipeline</param>
        /// <returns></returns>
        public MessengingPipeline GetPipelineFor(ICommunicationDevice device)
        {
            _ = device ?? throw new ArgumentNullException(nameof(device));
            return new MessengingPipeline(_messengerFactory.CreateMessangerUsing(device),
                                          _ingesterFactory.GetIngesterUsing(device.CompletitionTimeout),
                                          _processorFactory(),
                                          _confirmationTimeoutCheckerFactory.GetCheckerUsing(device.ConfirmationTimeout),
                                          _confirmationFactory());
        }

    }
}
