using Shield.Messaging.Commands;
using Shield.Messaging.Commands.States;

namespace Shield.Messaging.Protocol
{
    public class ConfirmationCommandTranslator
    {
        public Confirmation Translate(ICommand command)
        {
            return new Confirmation(command.Order.ToString(), ExtractErrors(command));
        }

        private ErrorState ExtractErrors(ICommand command)
        {
            return ErrorState.Custom(command.Data.ToString().Trim());
        }
    }
}