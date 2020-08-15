using Shield.Messaging.Protocol;

namespace Shield.Messaging.SlaveUnits
{
    public interface ISlaveUnit
    {
        string ID { get; set; }

        string Name { get; set; }

        bool IsConnected { get; set; }

        bool CanHandle(Order order);
    }
}