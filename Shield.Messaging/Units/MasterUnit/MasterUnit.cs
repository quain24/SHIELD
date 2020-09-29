using Shield.Messaging.Protocol;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shield.Messaging.Units.MasterUnit
{
    public class MasterUnit
    {
        private readonly Dictionary<string, IUnit> _attachedSlaveUnits = new Dictionary<string, IUnit>();
        private readonly ProtocolHandler _handler;

        public MasterUnit(ProtocolHandler handler)
        {
            _handler = handler;
        }

        public async Task<IDictionary<string, IUnit>> ReportAttachedSlaveUnits()
        {
            return null; // tmp
        }

        private async Task UpdateSlaveUnitDictionary(Order order)
        {
        }

        protected async Task<(bool isSuccess, Confirmation confirmation)> TrySendAndAwaitConfirmationAsync(Order order)
        {
            if (await SendAsync(order).ConfigureAwait(false) is false ||
                await _handler.Order().WasConfirmedInTimeAsync(order).ConfigureAwait(false) is false)
                return (false, null);

            return (true, _handler.Retrieve().ConfirmationOf(order));
        }

        protected async Task<(bool isSuccess, Reply reply)> TrySendAndAwaitReplyAsync(Order order)
        {
            if (await SendAsync(order).ConfigureAwait(false) is false ||
                await _handler.Order().WasRepliedToInTimeAsync(order).ConfigureAwait(false) is false)
                return (false, null);

            return (true, _handler.Retrieve().ReplyTo(order));
        }

        private async Task<bool> SendAsync(Order order)
        {
            var confirmation = await _handler.SendAsync(order);
            return confirmation.IsValid;
        }
    }
}