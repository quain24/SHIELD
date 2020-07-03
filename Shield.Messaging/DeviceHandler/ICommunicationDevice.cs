using System;

namespace Shield.Messaging.DeviceHandler
{
    public interface ICommunicationDevice : IDisposable
    {
        bool IsOpen { get; }
        string Name { get; }
        bool IsConnected { get; }
        bool IsReady { get; }

        void Open();

        void Close();

        void DiscardInBuffer();

        bool Send(string data);

        string Receive();
        bool IsPortExisting();

        event EventHandler<string> DataReceived;
    }
}