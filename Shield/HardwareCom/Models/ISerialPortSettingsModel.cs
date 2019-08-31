using System.IO.Ports;
using Shield.Data.Models;
using Shield.Enums;

namespace Shield.HardwareCom.Models
{
    public interface ISerialPortSettingsModel : ICommunicationDeviceSettings
    {
        int baudRate { get; set; }
        int DataBits { get; set; }
        Parity parity { get; set; }
        int portNumber { get; set; }
        StopBits stopBits { get; set; }
    }
}