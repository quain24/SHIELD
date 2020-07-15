
using System;
using System.Threading;
using System.Threading.Tasks;
using Timeout = Shield.Messaging.Commands.Timeout;

namespace Shield.Messaging.Protocol
{
    public class ChildAwaiter
    {
        private readonly Timeout _timeout;
        private readonly CancellationTokenSource _cts;
        public ChildAwaiter(Timeout timeout, CancellationTokenSource cts)
        {
            _timeout = timeout;
            _cts = cts;
        }

        public async Task<bool> RespondedInTime()
        {
            try
            {
                await Task.Delay(_timeout.InMilliseconds, _cts.Token).ConfigureAwait(false);
                return false;
            }
            catch(OperationCanceledException)
            {
                return true;
            }
        }
    }
}