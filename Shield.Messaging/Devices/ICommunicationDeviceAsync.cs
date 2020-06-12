using System.Threading;
using System.Threading.Tasks;

namespace Shield.Messaging.Devices
{
    public interface ICommunicationDeviceAsync : ICommunicationDevice
    {
        Task<bool> SendAsync(string data, CancellationToken ct = default);

        Task<string> ReceiveAsync(CancellationToken ct = default);
    }
}