using System;
using System.Threading;
using System.Threading.Tasks;
using Timeout = Shield.Messaging.Commands.Timeout;

namespace Shield.Messaging.Protocol
{
    public class ChildAwaiter : IChildAwaiter
    {
        private readonly Timeout _timeout;
        private readonly CancellationToken _ct;

        public ChildAwaiter(Timeout timeout, CancellationToken ct)
        {
            _timeout = timeout;
            _ct = ct;
        }

        public async Task<bool> HasRespondedInTimeAsync()
        {
            try
            {
                await Task.Delay(_timeout.InMilliseconds, _ct).ConfigureAwait(false);
                return false;
            }
            catch (OperationCanceledException)
            {
                return true;
            }
        }
    }
}