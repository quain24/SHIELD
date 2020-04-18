using Shield.HardwareCom.Models;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Shield.HardwareCom
{
    public interface IMessengingPipeline
    {
        bool IsOpen { get; }

        void Close();
        BlockingCollection<IMessageModel> GetReceivedMessages();
        void Open();
        Task<bool> SendAsync(IMessageModel message);
    }
}