using System.Collections.Generic;
using System.IO.Ports;
using System.Threading.Tasks;
using Shield.HardwareCom.Models;

namespace Shield.HardwareCom
{
    public interface IComMessanger
    {
        SerialPort Port { get; set; }

        void AddCommandTemp(ICommand command);
        Task Close();
        Task<List<string>> ReceiveAsync();
        void Send();
    }
}