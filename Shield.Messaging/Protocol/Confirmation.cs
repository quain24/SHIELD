using Shield.Messaging.Commands.States;
using Shield.Timestamps;
using System;

namespace Shield.Messaging.Protocol
{
    public class Confirmation : IResponseMessage
    {
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
            Confirms = confirmingID ?? throw new ArgumentNullException(nameof(confirmingID), "Cannot confirm a NULL id - target unknown.");
            ContainedErrors = errors ?? throw new ArgumentNullException(nameof(errors), "Error state has to be passed - either valid or not.");
            Timestamp = timestamp;
        }

        public string Target => Confirms;

        public bool IsValid => ContainedErrors == ErrorState.Unchecked().Valid();

        public string Confirms { get; }

        public Timestamp Timestamp { get; }

        public ErrorState ContainedErrors { get; }
    }
}