using System.Threading.Tasks;

namespace Shield.HardwareCom.MessageProcessing
{
    public interface ICompletitionTimeoutChecker
    {
        Task StartTimeoutCheckAsync();

        void StopTimeoutCheck();
    }
}