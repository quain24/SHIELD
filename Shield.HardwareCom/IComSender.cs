using System.IO.Ports;
using Shield.HardwareCom.Models;

namespace Shield.HardwareCom
{
    public interface IComSender
    {
        void Command(ICommandModel command);
        bool Send();
        void Setup(SerialPort port);
    }
}