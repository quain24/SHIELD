using Shield.Messaging.Commands.States;
using Shield.Timestamps;

namespace Shield.Messaging.Protocol
{
    public class ConfirmationFactory
    {
        public Confirmation Create(string confirmingId, ErrorState errors)
        {
            return Create(confirmingId, errors, Timestamp.Now);
        }

        public Confirmation Create(IConfirmable message, ErrorState errors)
        {
            return Create(message.ID, errors, Timestamp.Now);
        }

        public Confirmation Create(string confirmingId, ErrorState errors, Timestamp timestamp)
        {
            return new Confirmation(confirmingId, errors, timestamp);
        }
    }
}