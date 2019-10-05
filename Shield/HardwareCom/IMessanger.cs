using Shield.Enums;
using Shield.HardwareCom.Models;
using System.Threading.Tasks;

namespace Shield.HardwareCom
{
    public interface IMessanger : System.IDisposable
    {
        Task<bool> SendAsync(ICommandModel comand);

        Task<bool> SendAsync(IMessageModel message);

        bool Send(IMessageModel message);

        bool Setup(DeviceType type);

        void Open();

        void Close();

        Task StartReceiveAsync();

        void StopReceiving();
    }
}