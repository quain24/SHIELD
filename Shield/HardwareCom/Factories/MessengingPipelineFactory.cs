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
        private readonly IMessengingPipelineContextFactory _contextFactory;

        public MessengingPipelineFactory(IMessengingPipelineContextFactory contextFactory)
        {
            _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
        }

        /// <summary>
        /// Creates a <see cref="MessengingPipeline"/> for given <see cref="ICommunicationDevice"/> 
        /// </summary>
        /// <param name="device">Device used to send and receive data by pipeline</param>
        /// <returns></returns>
        public MessengingPipeline GetPipelineFor(ICommunicationDevice device)
        {
            _ = device ?? throw new ArgumentNullException(nameof(device));

            var context = _contextFactory.GetContextFor(device);
            return new MessengingPipeline(context);
        }

    }
}
