using Shield.CommonInterfaces;
using System;

namespace Shield.HardwareCom.Factories
{
    public class MessengingPipelineFactory : IMessengingPipelineFactory
    {
        private readonly IMessengingPipelineContextFactory _contextFactory;
        private readonly Func<IMessengingPipelineContext, IMessengingPipeline> _pipelineFactory;

        public MessengingPipelineFactory(IMessengingPipelineContextFactory contextFactory, Func<IMessengingPipelineContext, IMessengingPipeline> pipelineFactoryAF)
        {
            _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
            _pipelineFactory = pipelineFactoryAF;
        }

        /// <summary>
        /// Creates a <see cref="IMessengingPipeline"/> for given <see cref="ICommunicationDevice"/>
        /// </summary>
        /// <param name="device">Device used to send and receive data by pipeline</param>
        /// <returns>An instance of <see cref="IMessengingPipeline"/></returns>
        public IMessengingPipeline GetPipelineFor(ICommunicationDevice device)
        {
            _ = device ?? throw new ArgumentNullException(nameof(device));

            var context = _contextFactory.GetContextFor(device);
            return _pipelineFactory(context);//new MessengingPipeline(context);
        }
    }
}