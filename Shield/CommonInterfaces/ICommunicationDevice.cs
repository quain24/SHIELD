using Shield.HardwareCom.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shield.HardwareCom.CommonInterfaces
{
    public interface ICommunicationDevice : IDisposable
    {
        void Open();
        void Close();
        void DiscardInBuffer();
        string Read();
        void Write(string rawData);
        event EventHandler DataReceived;
    }
}
