using System.Collections.Generic;
using System.IO.Ports;

namespace Shield.HardwareCom.Factories
{
    public interface IComPortFactory
    {
        List<string> AvailablePorts { get; }
        SerialPort GivePort { get; }

        bool Create(int portNumber, int baudRate = 19200, int dataBits = 8, Parity parity = Parity.None, StopBits stopBits = StopBits.One);
    }
}