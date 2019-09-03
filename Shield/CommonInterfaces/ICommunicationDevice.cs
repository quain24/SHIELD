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
        void Send(ICommandModel command);
        event EventHandler<ICommandModel> DataReceived;
    }
}
