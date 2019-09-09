using Shield.CommonInterfaces;
using Shield.Enums;
using System.IO.Ports;

namespace Shield.HardwareCom.Factories
{
    public interface ICommunicationDeviceFactory
    {
        ICommunicationDevice Device(DeviceType typeOfDevice,
                                    int portNumber,
                                    int baudRate = 19200,
                                    int dataBits = 8,
                                    Parity parity = Parity.None,
                                    StopBits stopBits = StopBits.One);

        ICommunicationDevice Device(DeviceType type);
    }
}