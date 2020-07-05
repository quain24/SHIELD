using Shield.GlobalConfig;
using Shield.Messaging.Commands.Parts;
using System;

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
            _ = command ?? throw new ArgumentNullException(nameof(command),
                "Passed in NULL instead of a ICommand instance");
            return _commandFactory.Create(command.HostID,
                _partFactory.GetPart(Enums.Command.PartType.Order, ConfirmationTarget.ConfirmationTargetString),
                GenerateConfirmationDataPart(command));
        }

        private IPart GenerateConfirmationDataPart(ICommand command)
        {
            var data = command.ID.ToString() + " " + command.ErrorState;
            return _partFactory.GetPart(Enums.Command.PartType.Data, data);
        }
    }
}