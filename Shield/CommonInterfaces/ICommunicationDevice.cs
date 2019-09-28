using Shield.HardwareCom.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        event EventHandler<string> DataReceived;
    }
}
