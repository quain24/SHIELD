using Shield.Messaging.Protocol;

namespace Shield.Messaging.Units
{
    public interface IDiode : IUnit
    {
        Order FlashSingleDiode(int number, int intervalInMs, string color);

        Order FlashAllDiodes(int intervalInMs, string color);

        Order TurnOnDiode(int number, string color);

        Order TurnOffDiode(int number);
    }
}