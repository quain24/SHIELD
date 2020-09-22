using Shield.Messaging.Protocol;

namespace Shield.Messaging.Units
{
    public interface IUnit
    {
        string ID { get; }

        string Name { get; set; }

        bool IsConnected { get; }

        bool Reset();

        bool CanHandle(Order order);

        void ExecuteOrder(Order order);
    }
}