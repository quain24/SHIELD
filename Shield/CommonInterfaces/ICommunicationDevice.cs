using System;
using System.Threading.Tasks;

namespace Shield.CommonInterfaces
{
    public interface ICommunicationDevice : IDisposable
    {
        bool Setup(ICommunicationDeviceSettings settings);

        void Open();

        void Close();

        void DiscardInBuffer();

        Task<bool> SendAsync(string command);

        bool Send(string command);

        Task StartReceivingAsync();

        void StopReceiving();
        void StopSending();

        event EventHandler<string> DataReceived;
    }
}