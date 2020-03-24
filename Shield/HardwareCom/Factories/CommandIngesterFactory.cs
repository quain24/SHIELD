using Shield.HardwareCom.MessageProcessing;
using Shield.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shield.HardwareCom.Factories
{
    public class CommandIngesterFactory : ICommandIngesterFactory
    {
        private readonly Func<IMessageFactory> _messageFactory;
        private readonly Func<ICompleteness> _completnessFactory;
        private readonly Func<IIdGenerator> _idGeneratorFactory;
        private readonly ITimeoutCheckFactory _timeoutCheckFactory;

        public CommandIngesterFactory(Func<IMessageFactory> messageFactory, Func<ICompleteness> completnessFactory, Func<IIdGenerator> idGeneratorFactory, ITimeoutCheckFactory timeoutCheckFactory)
        {
            _messageFactory = messageFactory ?? throw new ArgumentNullException(nameof(messageFactory));
            _completnessFactory = completnessFactory ?? throw new ArgumentNullException(nameof(completnessFactory));
            _idGeneratorFactory = idGeneratorFactory ?? throw new ArgumentNullException(nameof(idGeneratorFactory));
            _timeoutCheckFactory = timeoutCheckFactory ?? throw new ArgumentNullException(nameof(timeoutCheckFactory));
        }
        // todo replaced CommandIngester with ALT version
        public ICommandIngesterAlt GetIngesterUsing(int timeout) =>
            new CommandIngesterAlt(_messageFactory(), _completnessFactory(), _timeoutCheckFactory.GetTimeoutCheckWithTimeoutSetTo(timeout), _idGeneratorFactory());
    }
}
