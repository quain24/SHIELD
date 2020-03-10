using Shield.Enums;
using Shield.HardwareCom.Models;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Shield.HardwareCom
{
    public interface IMessenger : System.IDisposable
    {
        bool IsOpen { get; }
        bool IsReceiving { get; }
        bool IsSending { get; }

        Task<bool> SendAsync(ICommandModel comand);

        void Open();

        void Close();

        Task StartReceiveingAsync(CancellationToken ct = default);

        void StopReceiving();

        Task<bool> SendAsync(IMessageModel message);
        BlockingCollection<ICommandModel> GetReceivedCommands();

        event System.EventHandler<ICommandModel> CommandReceived;
    }
}