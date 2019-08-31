using System.IO.Ports;
using Shield.Data.CommonInterfaces;

namespace Shield.Data.Models
{
    public interface ISerialPortSettingsModel : ICommunicationDeviceSettings
    {
        int BaudRate { get; set; }
        int DataBits { get; set; }
        Parity Parity { get; set; }
        int PortNumber { get; set; }
        StopBits StopBits { get; set; }
    }
}