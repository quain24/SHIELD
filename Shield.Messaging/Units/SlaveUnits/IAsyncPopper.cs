using System.Threading.Tasks;
using Shield.Messaging.Protocol;

namespace Shield.Messaging.Units.SlaveUnits
{
    internal interface IAsyncPopper : IUnit
    {
        Task<Order> FlashRedDiodesAsync(int milliseconds);

        Task<Order> FlashGreenDiodesAsync(int milliseconds);

        Task<Order> StartProgram();

        Task<Order> SetTimeLimit(int milliseconds);

        Task<Order> SetMaxHitNumber(int numberOfHits);

        Task<Order> ReportHits();
    }
}