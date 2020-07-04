using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shield.GlobalConfig;
using Shield.Messaging.Commands.Parts;

namespace Shield.Messaging.Commands
{
    public class ConfirmationFactory
    {
        private readonly CommandFactory _commandFactory;
        private readonly IPartFactory _partFactory;

        public ConfirmationFactory(CommandFactory commandFactory, IPartFactory partFactory)
        {
            _commandFactory = commandFactory ?? throw new ArgumentNullException(nameof(commandFactory));
            _partFactory = partFactory ?? throw new ArgumentNullException(nameof(partFactory));
        }

        public ICommand GetConfirmationFor(ICommand command)
        {
                return _commandFactory.Create(command.HostID,
                    _partFactory.GetPart(Enums.Command.PartType.Order, ConfirmationTarget.ConfirmationTargetString),
                    _partFactory.GetPart(Enums.Command.PartType.Data, command.ErrorState.ToString()));
        }
    }
}
