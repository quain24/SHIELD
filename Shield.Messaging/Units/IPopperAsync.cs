using System.Threading.Tasks;
using Shield.Messaging.Protocol;

namespace Shield.Messaging.Units
{
    internal interface IPopperAsync : IUnit
    {
        Task<Order> StartProgramAsync();

        Task<Order> SetTimeLimitAsync(int milliseconds);

        Task<Order> SetMaxHitNumberAsync(int numberOfHits);

        Task<Order> ReportHitsAsync();
    }
}