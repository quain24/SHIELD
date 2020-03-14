using Shield.CommonInterfaces;
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
        private readonly Func<ICommandIngester> _ingesterFactory;
        private readonly Func<IIncomingMessageProcessor> _processorFactory;

        public MessengingPipelineFactory(IMessengerFactory messengerFactory, Func<ICommandIngester> ingesterFactory, Func<IIncomingMessageProcessor> processorFactory)
        {
            _messengerFactory = messengerFactory ?? throw new ArgumentNullException(nameof(messengerFactory));
            _ingesterFactory = ingesterFactory ?? throw new ArgumentNullException(nameof(ingesterFactory));
            _processorFactory = processorFactory ?? throw new ArgumentNullException(nameof(processorFactory));
        }

        /// <summary>
        /// Creates a <see cref="MessengingPipeline"/> for given <see cref="ICommunicationDevice"/> 
        /// </summary>
        /// <param name="device">Device used to send and receive data by pipeline</param>
        /// <returns></returns>
        public MessengingPipeline GetPipelineFor(ICommunicationDevice device)
        {
            _ = device ?? throw new ArgumentNullException(nameof(device));
            return new MessengingPipeline(_messengerFactory.CreateMessangerUsing(device), _ingesterFactory(), _processorFactory());
        }

    }
}
