using System.Collections.Generic;
using System.IO.Ports;
using Shield.HardwareCom.Adapters;
using Shield.Data.Models;

namespace Shield.HardwareCom.Factories
{
    public interface ISerialPortAdapterFactory
    {
        List<string> AvailablePorts { get; }
        SerialPortAdapter GivePort { get; }

        bool Create(int portNumber,
                    int commandSize,
                    int baudRate = 19200,
                    int dataBits = 8,
                    Parity parity = Parity.None,
                    StopBits stopBits = StopBits.One);

        bool Create(ISerialPortSettingsModel settings);
    }
}