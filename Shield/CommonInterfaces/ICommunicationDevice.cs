using System;
using System.Threading;
using System.Threading.Tasks;

namespace Shield.CommonInterfaces
{
    public interface ICommunicationDevice : IDisposable
    {
        bool IsOpen { get; }

        bool Setup(ICommunicationDeviceSettings settings);

        void Open();

        void Close();

        Task CloseAsync();

        void DiscardInBuffer();

        Task<bool> SendAsync(string command, CancellationToken ct = default);

        bool Send(string command);

        Task<string> ReceiveAsync(CancellationToken ct = default);

        event EventHandler<string> DataReceived;
    }
}