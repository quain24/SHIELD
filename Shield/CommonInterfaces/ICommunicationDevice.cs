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
        Task<bool> SendAsync(ICommandModel command);
        bool Send(ICommandModel command);
        bool Setup(ICommunicationDeviceSettings settings);

        event EventHandler<ICommandModel> DataReceived;
    }
}
