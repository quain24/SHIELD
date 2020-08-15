using Shield.Messaging.Protocol;
using System;
using System.Threading.Tasks;

namespace Shield.Messaging.SlaveUnits
{
    internal class SimplePopper : AbstractSlaveUnit
    {
        private readonly ProtocolHandler _handler;

        public SimplePopper(ProtocolHandler handler) : base(handler)
        {
            _handler = handler;
        }

        public async Task<bool> FlashRedDiodesAsync(int milliseconds)
        {
            var order = Order.Create("fr", Name, milliseconds.ToString());
            var response = await base.TrySendAndAwaitConfirmationAsync(order).ConfigureAwait(false);
            return response.isSuccess && response.confirmation.IsValid;
        }

        public async Task<bool> FlashGreenDiodesAsync(int milliseconds)
        {
            var order = Order.Create("FG", Name, milliseconds.ToString());
            var response = await base.TrySendAndAwaitConfirmationAsync(order).ConfigureAwait(false);
            return response.isSuccess && response.confirmation.IsValid;
        }

        public void StartProgram()
        {
            throw new NotImplementedException();
        }

        public void SetTimeLimit(int milliseconds)
        {
            throw new NotImplementedException();
        }

        public void SetMaxHitNumber(int numberOfHits)
        {
            throw new NotImplementedException();
        }

        public string ReportHits()
        {
            throw new NotImplementedException();
        }
    }
}