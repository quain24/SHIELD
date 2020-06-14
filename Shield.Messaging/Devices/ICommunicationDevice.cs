using System;
using System.Threading.Tasks;

namespace Shield.Messaging.Devices
{
    public interface ICommunicationDevice : IDisposable
    {
        bool IsOpen { get; }
        string Name { get; }

        void Open();

        void Close();        

        void DiscardInBuffer();

        bool Send(string data);

        string Receive();

        event EventHandler<string> DataReceived;
    }
}