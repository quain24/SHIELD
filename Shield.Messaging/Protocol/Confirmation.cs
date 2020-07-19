using Shield.Messaging.Commands.States;
using Shield.Timestamps;

namespace Shield.Messaging.Protocol
{
    public class Confirmation : IResponseMessage
    {
        private readonly string _confirmingId;
        private readonly ErrorState _errors;
        private readonly Timestamp _timestamp;

        public Confirmation(string confirmingID, ErrorState errors, Timestamp timestamp)
        {
            _confirmingId = confirmingID;
            _errors = errors;
            _timestamp = timestamp;
        }

        public string Target => _confirmingId;

        public bool IsValid => _errors == ErrorState.Unchecked().Valid();

        public string Confirms => _confirmingId;
        public Timestamp Timestamp => _timestamp;
        public ErrorState ContainedErrors => _errors;
    }
}