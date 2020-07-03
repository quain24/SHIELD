using Shield.Messaging.Commands.Parts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shield.Messaging.Commands
{
    public class CommandFactoryAutoFacAdapter
    {
        private readonly Func<IPart, IPart, IPart, IPart, IPart, Timestamp, ICommand> _commandFactory;

        public CommandFactoryAutoFacAdapter(Func<IPart, IPart, IPart, IPart, IPart, Timestamp, ICommand> commandFactory)
        {
            _commandFactory = commandFactory ?? throw new ArgumentNullException(nameof(commandFactory));
        }

        public ICommand GetCommand(IPart hostID, IPart ID, IPart target, IPart order, IPart data, Timestamp timestamp)
        {
            return _commandFactory(hostID, ID, target, order, data, timestamp);
        }
    }
}
