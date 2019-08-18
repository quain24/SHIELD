using System.IO.Ports;

namespace CommunicationManager.Utilities
{
    public interface IComPortCommunicator
    {
        void SendData(string data);
        SerialPort GiveReceiver();
    }
}