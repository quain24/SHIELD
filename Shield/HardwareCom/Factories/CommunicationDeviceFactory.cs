﻿using Autofac.Features.Indexed;
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
        private IIndex<DeviceType, ICommunicationDevice> _deviceFactory;

        public CommunicationDeviceFactory(ISerialPortAdapterFactory serialAdapterFactory,
                                          IIndex<DeviceType, ICommunicationDevice> deviceFactory,
                                          IMoqAdapterFactory moqAdapterFactory,
                                          IAppSettings appSettings,
                                          Func<ICommandModel> commandModelFac)
        {
            _serialAdapterFactory = serialAdapterFactory;
            _moqAdapterFactory = moqAdapterFactory;
            _appSettings = appSettings;
            _commandModelFac = commandModelFac;
            _deviceFactory = deviceFactory;
        }
           
        public ICommunicationDevice Device(DeviceType type)
        {                       
            switch (type)
            {
               case DeviceType.Serial:
                    ISerialPortSettingsModel settings = (ISerialPortSettingsModel) _appSettings.GetSettingsFor(SettingsType.SerialDevice);
                    ICommunicationDevice device = _deviceFactory[type];
                    if(device.Setup(settings))
                        return device;
                    else
                        break;                    
               
               case DeviceType.Moq:
                    IMoqPortSettingsModel settings2 = (IMoqPortSettingsModel) _appSettings.GetSettingsFor(SettingsType.MoqDevice);
                    ICommunicationDevice device2 = _deviceFactory[type];
                    if(device2.Setup(settings2))
                       return device2;
                    else
                       break;

                default:
                    string err = $@"ERROR: CommunicationDeviceFactory Device - no device at ""{type}"" position in deviceType enum";
                    Debug.WriteLine(err);
                    return null;                    
            }
            return null;
        }

        // do usuniecia - kiedy bedzie koniec testow!
        public ICommunicationDevice Device(DeviceType typeOfDevice,
                                           int portNumber,
                                           int baudRate,
                                           int dataBits,
                                           Parity parity,
                                           StopBits stopBits)
        {
            switch (typeOfDevice)
            {
                case DeviceType.Serial:
                    if (_serialAdapterFactory.Create(portNumber, baudRate, dataBits, parity, stopBits))
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