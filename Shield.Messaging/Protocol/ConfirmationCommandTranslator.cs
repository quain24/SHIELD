using Shield.GlobalConfig;
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

        public ConfirmationCommandTranslator(IPartFactory partFactory, CommandFactory commandFactory)
        {
            _partFactory = partFactory;
            _commandFactory = commandFactory;
        }

        public ICommand Translate(Confirmation confirmation)
        {
            return _commandFactory.Create(
                _partFactory.GetPart(Command.PartType.Target, DefaultTargets.ConfirmationTarget),
                _partFactory.GetPart(Command.PartType.Order, confirmation.Confirms),
                _partFactory.GetPart(Command.PartType.Data, confirmation.ContainedErrors.ToString()));
        }

        public Confirmation Translate(ICommand confirmationCommand)
        {
            return new Confirmation(confirmationCommand.Order.ToString(), ExtractErrors(confirmationCommand), confirmationCommand.Timestamp);
        }

        private ErrorState ExtractErrors(ICommand command)
        {
            return ErrorState.Custom(command.Data.ToString().Trim());
        }
    }
}