using Shield.HardwareCom.Factories;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Threading.Tasks;
using Shield.HardwareCom.Models;
using Shield.CommonInterfaces;
using Shield.Enums;
using Shield.Data;
using Shield.Data.Models;
using System;

//  Metody wysyłania i odbierania sa uproszczone, TODO:
//  Po utworzeniu odpalamy setup, tam wybierany jest interfejs do komunikacji, konfigurowany jest automatycznie z danych z pliku setup.xml
//  
//  Send i receive beda pobierac i wysylac defaultowo jeden command - kazdy interfejs sam koduje i dekoduje raw data, messanger pracuje juz na gotowych commandach
//  tak zostawic czy przerabiamy?
//  
//  Czy tu odbierac sygnaly o czesciowych danych czy tylko o kompletnej komendzie?


namespace Shield.HardwareCom
{
    public class Messanger : IMessanger
    {
        private SerialPort _port = null;
        private ICommunicationDeviceFactory _communicationDeviceFactory;
        private IComSender _comSender;
        private IComReceiver _comReceiver;
        private int _bufferSize;
        private ICommunicationDevice _device;
        private IAppSettings _appSettings;

        public Messanger(IAppSettings appSettings, ICommunicationDeviceFactory communicationDeviceFactory)
        {
            _communicationDeviceFactory = communicationDeviceFactory;
            _appSettings = appSettings;
        }

        public int GetBuf {get{ return _bufferSize; } }

        public bool Setup(DeviceType type)
        {
            _device = _communicationDeviceFactory.Device(type);
            IApplicationSettingsModel appSettings = (IApplicationSettingsModel) _appSettings.GetSettingsFor(SettingsType.Application);

            if(_device is null || appSettings is null)
                return false;
                
            _bufferSize = appSettings.MessageSize;
            _device.DataReceived += DataReceivedEventHandler;
            return true;
        }

        // Do wywalenia, poki co jest do testow
        //
        //
        public Messanger(IAppSettings appSettings, ICommunicationDeviceFactory communicationDeviceFactory, IComSender comSender, IComReceiver comReceiver, int messageSizeBytes = 17)
        {
            _communicationDeviceFactory = communicationDeviceFactory;
            _comSender = comSender;
            _comReceiver = comReceiver;
            _bufferSize = messageSizeBytes;
            _appSettings = appSettings;
        }

        public bool Setup(DeviceType type, int additionalparameter)
        {
            _device = _communicationDeviceFactory.Device(type, 20, additionalparameter);            
            if( _device is null )
            {
                _comSender.Setup(_device);
                return true;
            }
            return false;
        }

        //do wywalenia - do testow
        public SerialPort Port
        {
            get
            {
                return _port;
            }
            set
            {
                _port = value;
                _comReceiver.Setup(_port, _bufferSize);
            }
        }

        // odpalany jezeli jakiekolwiek dane wpadly do interfejsu komunikacji
        // pomyslec nad zmianami - czy tu ma sie wlasciwie po co odpalac to, czy może w interfejsie partiale, 
        // a ten dopiero jak interfejs uzbiera pelen command??
        public void DataReceivedEventHandler(Object sender, EventArgs e )
        {
            _device.Receive();
        }

        //do wywalenia - testy - com receiver i sender wypadaja
        public List<string> Receive()
        {
            return _comReceiver.DataReceived;
        }

        // Testowa wersja metody async, domyslnie i tak caly com receiver bedzie async!
        public async Task<List<string>> ReceiveAsync()
        {
            return await Task.Run(() => _comReceiver.DataReceived).ConfigureAwait(false);
        }

        public void Send(ICommandModel command)
        {
            //_comSender.Send(command);
            _device.Open();
            _device.Send(command);
        }

        public Task Close()
        {
            // Close the serial port in a new thread
            Task closeTask = new Task(() =>
            {
                try
                {
                    _port.Close();
                }
                catch (IOException e)
                {
                    // Port was not open
                    Debug.WriteLine("Port was not open! " + e.Message);
                    throw e;
                }
            });
            closeTask.Start();

            return closeTask;

            // odniorca:
            // await serialStream.Close();
        }

        // do testow
        public void AddCommandTemp(ICommandModel command)
        {
            _comSender.Command(command);
        }       
    }
}