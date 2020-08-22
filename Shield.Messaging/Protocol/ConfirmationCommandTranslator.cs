using Shield.Messaging.Commands;
using Shield.Messaging.Commands.Parts;
using Shield.Messaging.Commands.States;
using Command = Shield.Enums.Command;

namespace Shield.Messaging.Protocol
{
    public class ConfirmationCommandTranslator
    {
        private readonly IPartFactory _partFactory;
        private readonly CommandFactory _commandFactory;
        private readonly ConfirmationFactory _confirmationFactory;

        public ConfirmationCommandTranslator(IPartFactory partFactory, CommandFactory commandFactory, ConfirmationFactory confirmationFactory)
        {
            _partFactory = partFactory;
            _commandFactory = commandFactory;
            _confirmationFactory = confirmationFactory;
        }

        public ICommand Translate(Confirmation confirmation)
        {
            return _commandFactory.Create(
                _partFactory.GetPart(Command.PartType.ID, "_"),
                _partFactory.GetPart(Command.PartType.Target, GlobalConfig.DefaultTargets.ConfirmationTarget),
                _partFactory.GetPart(Command.PartType.Order, confirmation.Confirms),
                _partFactory.GetPart(Command.PartType.Data, confirmation.ContainedErrors.ToString()));
        }

        public Confirmation Translate(ICommand confirmationCommand)
        {
            return _confirmationFactory.Create(
                confirmationCommand.Order.ToString(),
                ExtractErrors(confirmationCommand),
                confirmationCommand.Timestamp);
        }

        private ErrorState ExtractErrors(ICommand command)
        {
            return ErrorState.Custom(command.Data.ToString().Trim());
        }
    }
}