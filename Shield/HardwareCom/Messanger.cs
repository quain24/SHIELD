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
        private ICommunicationDeviceFactory _communicationDeviceFactory;
        private int _bufferSize;
        private ICommunicationDevice _device;
        private IAppSettings _appSettings;

        public Messanger(IAppSettings appSettings, ICommunicationDeviceFactory communicationDeviceFactory)
        {
            _communicationDeviceFactory = communicationDeviceFactory;
            _appSettings = appSettings;
        }

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

        public void Open()
        {
            _device.Open();
        }


        public void Close()
        {
            _device.Dispose();
        }

        // do sprawdzenia!
        int i = 0;

        public void DataReceivedEventHandler(object sender, ICommandModel e)
        {
            // tutaj zmiana do listy powiedzmy wiadomosci, ogolnie pomyslec co kiedy przejdziemy na gui - nie bedzie przeciez w konsoli wyswietlac
            // obecnie tylko wywala dane do konsoli
            i++;
            if (i % 1000 == 0)
                Console.WriteLine(e.CommandTypeString + " " + e.Data + " received signal");
        }

        public void ConstantReceive()
        {
            //_device.StartReceivingAsync();
            Task.Run(async () => await _device.ReadUsingStream());
        }   
        
        public async Task ConstantReceiveAsync()
        {
            await _device.StartReceivingAsync();
        }

        public async Task<bool> SendAsync(ICommandModel command)
        {
            bool result = await _device.SendAsync(command);
            return result;
        }

        // do zrobienia - sprawdzenia?
        public bool Send(ICommandModel command)
        {
            return _device.Send(command);
        }
    }
}