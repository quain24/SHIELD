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

        void DiscardInBuffer();

        Task<bool> SendAsync(string command);

        bool Send(string command);

        //Task StartReceivingAsync();
        Task<string> ReceiveAsync(CancellationToken cancellToken);

        void StopReceiving();

        void StopSending();

        event EventHandler<string> DataReceived;
    }
}