using System;
using Shield.Messaging.Commands.States;
using Shield.Timestamps;

namespace Shield.Messaging.Protocol
{
    public class Confirmation : IResponseMessage
    {
        private readonly string _confirmingId;
        private readonly ErrorState _errors;
        private readonly Timestamp _timestamp;

        public static Confirmation Create(string confirmingID, ErrorState errors, Timestamp timestamp)
        {
            return new Confirmation(confirmingID, errors, timestamp);
        }

        public static Confirmation Create(string confirmingID, ErrorState errors)
        {
            return new Confirmation(confirmingID, errors, Timestamp.Now);
        }

        public Confirmation(string confirmingID, ErrorState errors, Timestamp timestamp)
        {
            _confirmingId = confirmingID ?? throw new ArgumentNullException(nameof(confirmingID), "Cannot confirm a NULL id - target unknown.");
            _errors = errors ?? throw new ArgumentNullException(nameof(errors), "Error state has to be passed - either valid or not.");
            _timestamp = timestamp;
        }

        public string Target => _confirmingId;

        public bool IsValid => _errors == ErrorState.Unchecked().Valid();

        public string Confirms => _confirmingId;
        public Timestamp Timestamp => _timestamp;
        public ErrorState ContainedErrors => _errors;
    }
}