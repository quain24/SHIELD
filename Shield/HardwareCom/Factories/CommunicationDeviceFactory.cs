using Autofac.Features.Indexed;
using Shield.CommonInterfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shield.Enums;
using Shield.HardwareCom.Adapters;
using System.IO.Ports;
using Shield.Data;
using Shield.Data.Models;
using Shield.HardwareCom.Models;

namespace Shield.HardwareCom.Factories
{
    /// <summary>
    /// Gives a choosen type of communication device based on DeviceType provided
    /// Takes optional parameters or a configuration directly from AppSettings (loaded from file)
    /// </summary>

    public class CommunicationDeviceFactory : ICommunicationDeviceFactory
    {
        private readonly ISerialPortAdapterFactory _serialAdapterFactory;
        private readonly IMoqAdapterFactory _moqAdapterFactory;
        private IAppSettings _appSettings;
        private Func<ICommandModel> _commandModelFac;

        public CommunicationDeviceFactory(ISerialPortAdapterFactory serialAdapterFactory,
                                          IMoqAdapterFactory moqAdapterFactory,
                                          IAppSettings appSettings,
                                          Func<ICommandModel> commandModelFac)
        {
            _serialAdapterFactory = serialAdapterFactory;
            _moqAdapterFactory = moqAdapterFactory;
            _appSettings = appSettings;
            _commandModelFac = commandModelFac;
        }
           
        public ICommunicationDevice Device(DeviceType type)
        {                       // To trzeba przerobic po pozbyciu sie create bioracej parametry w konstruktorze - zbedne poza testami
            switch (type)
            {
                case DeviceType.Serial:
                    ISerialPortSettingsModel settings = (ISerialPortSettingsModel) _appSettings.GetSettingsFor(SettingsType.SerialDevice);
                    return Device(type, settings.PortNumber, settings.CommandSize, settings.BaudRate, settings.DataBits, settings.Parity, settings.StopBits);
                
                case DeviceType.Moq:
                    IMoqPortSettingsModel settings2 = (IMoqPortSettingsModel) _appSettings.GetSettingsFor(SettingsType.MoqDevice);
                    return Device(type, settings2.PortNumber, 0, 0, 0, 0, 0);

                default:
                    string err = "ERROR: CommunicationDeviceFactory Device - could not create a communication device - maybe bad enum value or additional data?";
                    Debug.WriteLine(err);
                    return null;                    
            }
        }

        public ICommunicationDevice Device(DeviceType typeOfDevice,
                                           int portNumber,
                                           int commandSize,
                                           int baudRate,
                                           int dataBits,
                                           Parity parity,
                                           StopBits stopBits)
        {
            switch (typeOfDevice)
            {
                case DeviceType.Serial:
                    if (_serialAdapterFactory.Create(portNumber, commandSize, baudRate, dataBits, parity, stopBits))
                        return _serialAdapterFactory.GivePort;
                    break;

                case DeviceType.Moq:
                    if (_moqAdapterFactory.Create(portNumber))
                        return _moqAdapterFactory.GivePort;   
                    break;
            }

            string err = "ERROR: CommunicationDeviceFactory Device - could not create a communication device - maybe bad enum value or additional data?";
            Debug.WriteLine(err);
            return null;
        }        
    }
}
