using System;
using System.Threading.Tasks;
using Shield.Messaging.Protocol;

namespace Shield.Messaging.Units.SlaveUnits
{
    public abstract class AbstractSlaveUnit
    {

        protected AbstractSlaveUnit()
        {
        }

        public string ID { get; protected set; }
        public string Name { get; set; }
        public bool IsConnected { get; protected set; }
        public abstract bool CanHandle(Order order);
        public abstract Task HandleIncomingOrderAsync(Order order);
    }
}