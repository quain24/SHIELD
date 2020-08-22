using Shield.Messaging.Commands.Parts;
using System;
using Shield.Timestamps;

namespace Shield.Messaging.Commands
{
    public class CommandFactoryAutoFacAdapter
    {
        private readonly Func<IPart, IPart, IPart, IPart, IPart, Timestamp, ICommand> _commandFactory;

        public CommandFactoryAutoFacAdapter(Func<IPart, IPart, IPart, IPart, IPart, Timestamp, ICommand> commandFactory)
        {
            _commandFactory = commandFactory ?? throw new ArgumentNullException(nameof(commandFactory));
        }

        public ICommand GetCommand(IPart ID, IPart hostID, IPart target, IPart order, IPart data, Timestamp timestamp)
        {
            return _commandFactory(ID, hostID, target, order, data, timestamp);
        }
    }
}