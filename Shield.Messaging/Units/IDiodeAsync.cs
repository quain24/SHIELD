using System.Threading.Tasks;
using Shield.Messaging.Protocol;

namespace Shield.Messaging.Units
{
    public interface IDiodeAsync : IUnit
    {
        Task<Order> FlashSingleDiodeAsync(int number, int intervalInMs, string color);

        Task<Order> FlashAllDiodesAsync(int intervalInMs, string color);

        Task<Order> TurnOnDiodeAsync(int number, string color);

        Task<Order> TurnOffDiodeAsync(int number);
    }
}