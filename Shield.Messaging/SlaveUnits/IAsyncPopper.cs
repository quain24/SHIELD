using Shield.Messaging.Protocol;
using System.Threading.Tasks;

namespace Shield.Messaging.SlaveUnits
{
    internal interface IAsyncPopper : ISlaveUnit
    {
        Task<Order> FlashRedDiodesAsync(int milliseconds);

        Task<Order> FlashGreenDiodesAsync(int milliseconds);

        Task<Order> StartProgram();

        Task<Order> SetTimeLimit(int milliseconds);

        Task<Order> SetMaxHitNumber(int numberOfHits);

        Task<Order> ReportHits();
    }
}