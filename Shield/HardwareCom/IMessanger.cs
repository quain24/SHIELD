using System.Collections.Generic;
using System.IO.Ports;
using System.Threading.Tasks;
using Shield.Enums;
using Shield.HardwareCom.Models;

namespace Shield.HardwareCom
{
    public interface IMessanger
    {
        void Send(ICommandModel comand);
        bool Setup(DeviceType type);

        void Open();
        void Close();
    }
}