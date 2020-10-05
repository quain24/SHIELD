using Shield.Messaging.Protocol;
using System;
using System.Threading.Tasks;

namespace Shield.Messaging.Units.SlaveUnits
{
    internal class SimplePopper : AbstractSlaveUnit, IPopperAsync, IDiodeAsync
    {
        private readonly IMasterUnit _master;
        private readonly OrderFactory _orderFactory;

        public SimplePopper(IMasterUnit master, OrderFactory orderFactory)
        {
            _master = master;
            _orderFactory = orderFactory;
        }

        public bool Reset()
        {
            throw new NotImplementedException();
        }

        public void ExecuteOrder(Order order)
        {
            throw new NotImplementedException();
        }

        public Task<Order> StartProgramAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Order> SetTimeLimitAsync(int milliseconds)
        {
            throw new NotImplementedException();
        }

        public Task<Order> SetMaxHitNumberAsync(int numberOfHits)
        {
            throw new NotImplementedException();
        }

        public Task<Order> ReportHitsAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Order> FlashSingleDiodeAsync(int number, int intervalInMs, string color)
        {
            throw new NotImplementedException();
        }

        public Task<Order> FlashAllDiodesAsync(int intervalInMs, string color)
        {
            throw new NotImplementedException();
        }

        public Task<Order> TurnOnDiodeAsync(int number, string color)
        {
            throw new NotImplementedException();
        }

        public Task<Order> TurnOffDiodeAsync(int number)
        {
            throw new NotImplementedException();
        }

        public override bool CanHandle(Order order)
        {
            throw new NotImplementedException();
        }
    }
}