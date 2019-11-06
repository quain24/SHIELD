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

        Task<bool> SendAsync(ICommandModel comand);

        Task<bool> SendAsync(IMessageModel message);

        bool Send(IMessageModel message);

        bool Setup(DeviceType type);

        void Open();

        void Close();

        Task StartReceiveAsync(CancellationToken ct = default);

        void StopReceiving();

        Task StartDecodingAsync(CancellationToken ct = default);

        void StopDecoding();

        event System.EventHandler<ICommandModel> CommandReceived;
    }
}