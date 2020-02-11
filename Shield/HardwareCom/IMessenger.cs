using Shield.Enums;
using Shield.HardwareCom.Models;
using System.Threading;
using System.Threading.Tasks;

namespace Shield.HardwareCom
{
    public interface IMessanger : System.IDisposable
    {
        bool IsOpen { get; }
        bool IsReceiving { get; }
        bool IsDecoding { get; }

        bool IsSending { get; }

        Task<bool> SendAsync(ICommandModel comand);

        bool Setup(DeviceType type);

        void Open();

        void Close();

        Task StartReceiveingAsync(CancellationToken ct = default);

        void StopReceiving();

        void StartDecoding(CancellationToken ct = default);

        void StopDecoding();
        Task<bool> SendAsync(IMessageModel message);

        event System.EventHandler<ICommandModel> CommandReceived;
    }
}