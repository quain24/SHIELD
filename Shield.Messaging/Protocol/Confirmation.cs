using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shield.Messaging.Commands.States;

namespace Shield.Messaging.Protocol
{
    public class Confirmation
    {
        private readonly string _confirmingId;
        private readonly ErrorState _errors;

        public Confirmation(string confirmingID, ErrorState errors)
        {
            _confirmingId = confirmingID;
            _errors = errors;
        }

        public bool IsValid => _errors == ErrorState.Unchecked().Valid();

        public string Confirms => _confirmingId;
        public ErrorState ContainedErrors => _errors;
    }
}
