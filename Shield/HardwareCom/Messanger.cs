using Shield.CommonInterfaces;
using Shield.Data;
using Shield.Data.Models;
using Shield.Enums;
using Shield.HardwareCom.Factories;
using Shield.HardwareCom.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shield.HardwareCom
{
    public class Messanger : IMessanger
    {
        private ICommunicationDeviceFactory _communicationDeviceFactory;
        private ICommunicationDevice _device;
        private IAppSettings _appSettings;
        private ICommandTranslator _commandTranslator;

        private List<ICommandModel> _tempCommandsList = new List<ICommandModel>();
        private bool _setupSuccessufl = false;

        public Messanger(IAppSettings appSettings, ICommunicationDeviceFactory communicationDeviceFactory, ICommandTranslator commandTranslator)
        {
            _communicationDeviceFactory = communicationDeviceFactory;
            _appSettings = appSettings;
            _commandTranslator = commandTranslator;
        }

        public bool Setup(DeviceType type)
        {
            _device = _communicationDeviceFactory.Device(type);
            IApplicationSettingsModel appSettings = _appSettings.GetSettingsFor<IApplicationSettingsModel>();

            if (_device is null || appSettings is null)
                return _setupSuccessufl = false;

            _device.DataReceived += OnDataReceived;
            _setupSuccessufl = true;
            return _setupSuccessufl;
        }

        public void Open()
        {
            if (_setupSuccessufl)
                _device.Open();
        }

        public void Close()
        {
            _device?.Dispose();
        }

        // do sprawdzenia!
        private int i = 0;

        public void OnDataReceived(object sender, string e)
        {
            // tutaj zmiana do listy powiedzmy wiadomosci, ogolnie pomyslec co kiedy przejdziemy na gui - nie bedzie przeciez w konsoli wyswietlac
            // obecnie tylko wywala dane do konsoli

            i++;
            _tempCommandsList.Add(_commandTranslator.FromString(e));
            //if (i % 100 == 0)
            Console.WriteLine(_tempCommandsList[_tempCommandsList.Count - 1].CommandTypeString + " " + _tempCommandsList[_tempCommandsList.Count - 1].Id + " " + _tempCommandsList[_tempCommandsList.Count - 1].Data + " received (" + i + ") messages");
        }

        public async Task ConstantReceiveAsync()
        {
            if(_setupSuccessufl)
                await Task.Run(() => _device.StartReceivingAsync());
        }

        public async Task<bool> SendAsync(ICommandModel command)
        {
            bool result = await _device.SendAsync(_commandTranslator.FromCommand(command));
            return result;
        }

        // do zrobienia - sprawdzenia?
        public bool Send(IMessageModel message)
        {
            foreach (ICommandModel c in message)
            {
                if (_device.Send(_commandTranslator.FromCommand(c)))
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

                foreach (ICommandModel c in message)
                {
                    results.Add(await _device.SendAsync(_commandTranslator.FromCommand(c)).ConfigureAwait(false));
                }

                if (results.Contains(false))
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