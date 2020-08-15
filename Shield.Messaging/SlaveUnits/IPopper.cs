using System.Threading.Tasks;

namespace Shield.Messaging.SlaveUnits
{
    internal interface IPopper : ISlaveUnit
    {
        Task<bool> FlashRedDiodesAsync(int milliseconds);

        Task<bool> FlashGreenDiodesAsync(int milliseconds);

        void StartProgram();

        void SetTimeLimit(int milliseconds);

        void SetMaxHitNumber(int numberOfHits);

        string ReportHits();
    }
}