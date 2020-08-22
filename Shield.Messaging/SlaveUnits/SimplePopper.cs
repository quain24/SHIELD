using Shield.Messaging.Protocol;
using System;
using System.Threading.Tasks;

namespace Shield.Messaging.SlaveUnits
{
    internal class SimplePopper : AbstractSlaveUnit
    {
        private readonly OrderFactory _orderFactory;

        public SimplePopper(ProtocolHandler handler, OrderFactory orderFactory) : base(handler, orderFactory)
        {
            _orderFactory = orderFactory;
        }

        public Order FlashRedDiodesAsync(int intervalMilliseconds)
        {
            return _orderFactory.Create("fr", Name, new IntDataPack(intervalMilliseconds));
        }

        public Order FlashGreenDiodesAsync(int intervalMilliseconds)
        {
            return _orderFactory.Create("fg", Name, new IntDataPack(intervalMilliseconds));
        }

        public Order StartProgram()
        {
            return _orderFactory.Create("start", Name, new EmptyDataPack());
        }

        public void SetTimeLimit(int milliseconds)
        {
            throw new NotImplementedException();
        }

        public void SetMaxHitNumber(int numberOfHits)
        {
            throw new NotImplementedException();
        }

        public async Task UpdateState()
        {
            throw new NotImplementedException();
        }

        // Invokable methods

        private void TurnGreenOn()
        {
        }

        private void TurnGreenOff()
        {
        }
    }
}