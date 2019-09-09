using System.Collections.Generic;
using System.IO.Ports;
using Shield.HardwareCom.Adapters;
using Shield.Data.Models;
using Shield.CommonInterfaces;

namespace Shield.HardwareCom.Factories
{
    public interface ISerialPortAdapterFactory
    {
        List<string> AvailablePorts { get; }
        ICommunicationDevice GivePort { get; }

        bool Create(int portNumber,
                    int baudRate = 19200,
                    int dataBits = 8,
                    Parity parity = Parity.None,
                    StopBits stopBits = StopBits.One);

        bool Create(ISerialPortSettingsModel settings);
    }
}