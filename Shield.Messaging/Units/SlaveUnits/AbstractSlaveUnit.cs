using Shield.Messaging.Protocol;
using System;
using System.Threading.Tasks;

namespace Shield.Messaging.Units.SlaveUnits
{
    public abstract class AbstractSlaveUnit
    {
        protected AbstractSlaveUnit(string id)
        {
            ID = id ?? throw new ArgumentNullException(nameof(id), "ID has to be provided");
        }

        public string ID { get; }
        public string Name { get; set; }
        public bool IsConnected { get; protected set; }

        public abstract bool CanHandle(Order order);

        public abstract Task HandleIncomingOrderAsync(Order order);
    }
}