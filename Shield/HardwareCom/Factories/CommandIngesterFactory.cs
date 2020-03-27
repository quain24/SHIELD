using Shield.HardwareCom.CommandProcessing;
using Shield.HardwareCom.MessageProcessing;
using Shield.Helpers;
using System;

namespace Shield.HardwareCom.Factories
{
    public class CommandIngesterFactory : ICommandIngesterFactory
    {
        private readonly Func<IMessageFactory> _messageFactory;
        private readonly Func<ICompleteness> _completnessFactory;
        private readonly Func<IIdGenerator> _idGeneratorFactory;

        public CommandIngesterFactory(Func<IMessageFactory> messageFactory, Func<ICompleteness> completnessFactory, Func<IIdGenerator> idGeneratorFactory)
        {
            _messageFactory = messageFactory ?? throw new ArgumentNullException(nameof(messageFactory));
            _completnessFactory = completnessFactory ?? throw new ArgumentNullException(nameof(completnessFactory));
            _idGeneratorFactory = idGeneratorFactory ?? throw new ArgumentNullException(nameof(idGeneratorFactory));
        }

        public ICommandIngester GetIngesterUsing(IIdGenerator idGenerator) =>
            new CommandIngester(_messageFactory(), _completnessFactory(), idGenerator);

        public ICommandIngester GetIngetster() => 
            new CommandIngester(_messageFactory(), _completnessFactory(), _idGeneratorFactory());
    }
}