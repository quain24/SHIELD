using Shield.Messaging.Protocol;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
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
        protected override IEnumerable MethodsInvokableByOrders()
        {
            // Get table of func delegates of input data, confirmation - all incoming order processors will have the same signature
            // Every method will have to check input data itself.
            // Register all those methods in separate object (incoming order handler)
            // received order -> handler checks for possible target -> invocation of target -> response - confirmation of some sort
            // if reply is required then called method should generate one and send it;
            // Possibly create separate replier object
        }

        public async Task<bool> FlashRedDiodesAsync(int milliseconds)
        {
            var order = Order.Create("fr", Name, milliseconds.ToString());
            var (isSuccess, confirmation) = await base.TrySendAndAwaitConfirmationAsync(order).ConfigureAwait(false);
            return isSuccess && confirmation.IsValid;
        }

        public async Task<bool> FlashGreenDiodesAsync(int milliseconds)
        {
            var order = Order.Create("fg", Name, milliseconds.ToString());
            var (isSuccess, confirmation) = await base.TrySendAndAwaitConfirmationAsync(order).ConfigureAwait(false);
            return isSuccess && confirmation.IsValid;
        }

        public async Task<bool> StartProgram()
        {
            var order = Order.Create("start", Name);
            var (isSuccess, confirmation) = await base.TrySendAndAwaitConfirmationAsync(order).ConfigureAwait(false);
            return isSuccess && confirmation.IsValid;
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

        // Invokable methods

        private void Blink()
        {

        }

    }
}