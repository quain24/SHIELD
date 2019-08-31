using Autofac.Features.Indexed;
using Shield.HardwareCom.CommonInterfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shield.Enums;
using Shield.HardwareCom.Adapters;
using System.IO.Ports;

namespace Shield.HardwareCom.Factories
{
    // Fabryka ta zwraca potrzebne interfejsy komunikacyjne (porty), gotowe do otwarcia, po inicjalizacji
    // Jest nadrzędna do serialAdapterFactory i podobnych

    public class CommunicationDeviceFactory : ICommunicationDeviceFactory
    {
        private readonly ISerialPortAdapterFactory _serialAdapterFactory;
        private readonly IMoqAdapterFactory _moqAdapterFactory;

        public CommunicationDeviceFactory(ISerialPortAdapterFactory serialAdapterFactory, IMoqAdapterFactory moqAdapterFactory)
        {
            _serialAdapterFactory = serialAdapterFactory;
            _moqAdapterFactory = moqAdapterFactory;
        }
                

        public ICommunicationDevice Device(DeviceType typeOfDevice,
                                           int portNumber,
                                           int baudRate,
                                           int dataBits,
                                           Parity parity,
                                           StopBits stopBits)
        {
            switch (typeOfDevice)
            {
                case DeviceType.serial:
                    if (_serialAdapterFactory.Create(portNumber, baudRate, dataBits, parity, stopBits))
                        return _serialAdapterFactory.GivePort;
                    break;

                case DeviceType.moq:
                    if (_moqAdapterFactory.Create(portNumber))
                        return _moqAdapterFactory.GivePort;   
                    break;
            }

            string err = "ERROR: CommunicationDeviceFactory - could not create a communication device - maybe bad enum value or additional data?";
            Debug.WriteLine(err);
            throw new ArgumentException(err);
        }        
    }
}
