using System;
using Shield.Messaging.Protocol;

namespace Shield.Messaging.Units
{
    public abstract class BaseUnit
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public bool IsConnected { get; set; }

        public bool CanHandle(Order order)
        {
            throw new NotImplementedException();
        }
    }
}