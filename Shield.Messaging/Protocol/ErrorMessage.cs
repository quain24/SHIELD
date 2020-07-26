using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shield.Extensions;
using Shield.Messaging.Commands.States;
using Shield.Timestamps;

namespace Shield.Messaging.Protocol
{
    public class ErrorMessage
    {
        private readonly string[] _rawParts;
        private readonly ErrorState _errorState;
        private readonly Timestamp _timestamp;

        public ErrorMessage(ErrorState errorState, Timestamp timestamp, params string[] rawParts)
        {
            _rawParts = rawParts.IsNullOrEmpty() ? Array.Empty<string>() : rawParts;
            _errorState = errorState;
            _timestamp = timestamp;
        }

        public string[] Data => _rawParts;
        public Timestamp Timestamp => _timestamp;
        public ErrorState ErrorState => _errorState;
    }
}
