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
        void Open();
        void Close();
        void DiscardInBuffer();
        ICommandModel Receive();
        Task StartReceiving();
        Task StartReceivingAsync();
        Task<bool> SendAsync(ICommandModel command);
        bool Send(ICommandModel command);
        bool Setup(ICommunicationDeviceSettings settings);

        Task<int> ReadUsingStream();

        event EventHandler<ICommandModel> DataReceived;
    }
}
