using System.IO.Ports;
using Shield.HardwareCom.Models;

namespace Shield.HardwareCom
{
    public interface IComSender
    {
        void Command(ICommandModel command);
        bool Send(ICommandModel command);
        //void Setup(SerialPort port);
        void Setup(CommonInterfaces.ICommunicationDevice port);
    }
}