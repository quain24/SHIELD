using System.Threading.Tasks;

namespace Shield.HardwareCom.MessageProcessing
{
    public interface ICompletitionTimeoutChecker
    {
        bool IsWorking { get; }

        Task StartTimeoutCheckAsync();

        void StopTimeoutCheck();
    }
}