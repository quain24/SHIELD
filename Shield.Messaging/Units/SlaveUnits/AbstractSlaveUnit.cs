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

        public string ID { get; set; }
        public string Name { get; set; }
        public bool IsConnected { get; set; }
        public bool CanHandle(Order order)
        {
            throw new NotImplementedException();
        }
    }
}