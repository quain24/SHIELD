using System.IO.Ports;
using Shield.Data.Models;
using Shield.Enums;

namespace Shield.HardwareCom.Models
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