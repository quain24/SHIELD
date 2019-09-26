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
        private ICommunicationDevice _device;
        private IAppSettings _appSettings;

        private List<ICommandModel> _tempCommandsList = new List<ICommandModel>();

        private bool _setupSuccessufl = false;

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
                return _setupSuccessufl = false;

            _device.DataReceived += OnDataReceived;
            _setupSuccessufl = true;
            return _setupSuccessufl;
        }

        public void Open()
        {
            if(_setupSuccessufl)
                _device.Open();
        }

        public void Close()
        {
            _device?.Dispose();
        }

        // do sprawdzenia!
        int i = 0;

        public void OnDataReceived(object sender, ICommandModel e)
        {
            // tutaj zmiana do listy powiedzmy wiadomosci, ogolnie pomyslec co kiedy przejdziemy na gui - nie bedzie przeciez w konsoli wyswietlac
            // obecnie tylko wywala dane do konsoli
            i++;
            _tempCommandsList.Add(e);
            if (i % 100 == 0)
                Console.WriteLine(e.CommandTypeString + " " + e.Id + " " + e.Data + " received (" + i + ") messages" );
        }

        public async Task ConstantReceiveAsync()
        {
           await Task.Run( () =>  _device.StartReceivingAsync());
        } 

        public async Task<bool> SendAsync(ICommandModel command)
        {
            bool result = await _device.SendAsync(command);
            return result;
        }

        // do zrobienia - sprawdzenia?
        public bool Send(IMessageModel message)
        {
            foreach(ICommandModel c in message)
            {
                if(_device.Send(c))
                    continue;
                else
                    return false;
            }
            return true;
        }
        public async Task<bool> SendAsync(IMessageModel message)
        {
            // tak sobie na szybko, do przemyslenia, do poprawy

            return await Task.Run(async () =>
            {
                List<bool> results = new List<bool>();

                foreach(ICommandModel c in message)
                {
                    results.Add(await _device.SendAsync(c).ConfigureAwait(false)); 
                }

                if(results.Contains(false))
                    return false;
                return true;
            });



            //List<Task<bool>> tmp = new List<Task<bool>>();            

            //foreach(ICommandModel c in message)
            //{
            //    tmp.Add(_device.SendAsync(c));   
            //}

            //bool[] tmpbool = new bool[message.CommandCount];

            //tmpbool = await Task.WhenAll<bool>(tmp);
            //foreach(bool b in tmpbool)
            //{
            //    if(!b)
            //        return false;
            //}

            //return true;
        }
    }
}