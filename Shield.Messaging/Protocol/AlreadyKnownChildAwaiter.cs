using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shield.Messaging.Protocol
{
    class AlreadyKnownChildAwaiter : IChildAwaiter
    {
        private readonly bool _alreadyKnown;

        public AlreadyKnownChildAwaiter(bool alreadyKnown)
        {
            _alreadyKnown = alreadyKnown;
        }
        public Task<bool> HasRespondedInTimeAsync()
        {
            return Task.FromResult(_alreadyKnown);
        }
    }
}
