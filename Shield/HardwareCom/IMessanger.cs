using System.Collections.Generic;
using System.IO.Ports;
using System.Threading.Tasks;
using Shield.Enums;
using Shield.HardwareCom.Models;

namespace Shield.HardwareCom
{
    public interface IMessanger
    {
        SerialPort Port { get; set; }

        void AddCommandTemp(ICommandModel command);
        Task Close();
        Task<List<string>> ReceiveAsync();
        void Send(ICommandModel comand);
        bool Setup(DeviceType type, int additionalparameter);
        bool Setup(DeviceType type);
        int GetBuf {get; }
    }
}