using Shield.CommonInterfaces;
using Shield.Data;
using Shield.Data.Models;
using Shield.Enums;
using Shield.HardwareCom.Factories;
using Shield.HardwareCom.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Shield.Extensions;

namespace Shield.HardwareCom
{
    // TODO
    // wywalilem na zewnatrz z serial port adaptera przeczesywanie stringow do tłumaczenia do messangera
    // jest do tego urzyta nowa klasa - trzeba ja zoptymalizowac i wprowadzic cancelacje, teraz dziala non stop w innym watku
    // sprawdzic, zoptymalizowac, sprawdzic, wywalic dokumentnie na koncu jak zadziala z serial port adapter 


    public class Messanger : IMessanger
    {
        private ICommunicationDeviceFactory _communicationDeviceFactory;
        private ICommunicationDevice _device;
        private IAppSettings _appSettings;
        private ICommandTranslator _commandTranslator;
        private IIncomingDataPreparer _incomingDataPreparer;

        private Regex _commandPattern;
        private BlockingCollection<string> _rawDataBuffer = new BlockingCollection<string>();
        private List<ICommandModel> _tempCommandsList = new List<ICommandModel>();
        private bool _setupSuccessufl = false;

        //private IncomingDataPreparer _idp = new IncomingDataPreparer();

        public Messanger(IAppSettings appSettings, ICommunicationDeviceFactory communicationDeviceFactory, ICommandTranslator commandTranslator, IIncomingDataPreparer incomingDataPreparer)
        {
            _communicationDeviceFactory = communicationDeviceFactory;
            _appSettings = appSettings;
            _commandTranslator = commandTranslator;
            _incomingDataPreparer = incomingDataPreparer;
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
            _device?.Close();
        }

        // testowanie nowej wersji (wyciagnietej z serialportadapter) extraktora danych

        private async Task TestDataExtractor()
        {
            int i = 0;
            while (true)
            {
                if(_rawDataBuffer.Count > 0)
                {
                    List<string> output = _incomingDataPreparer.DataSearch(_rawDataBuffer.Take());
                    foreach(string s in output)
                    {
                        ICommandModel com =  _commandTranslator.FromString(s);
                        Console.WriteLine(com.CommandTypeString + " " + com.Id + " " + com.Data + " | Received by external data searcher (" + i++ + ")" );
                    }
                }
                else
                {
                    await Task.Delay(50);
                }
            }
        }

        // do sprawdzenia!
        private int i = 0;

        public void OnDataReceived(object sender, string e)
        {
            // tutaj zmiana do listy powiedzmy wiadomosci, ogolnie pomyslec co kiedy przejdziemy na gui - nie bedzie przeciez w konsoli wyswietlac
            // obecnie tylko wywala dane do konsoli

            i++;
            //_tempCommandsList.Add(_commandTranslator.FromString(e));
            _rawDataBuffer.Add(e.RemoveASCIIChars());
        }

        // napisac l;ogiczna metode cancelacji, jakis cancelation token, dodac using tak, by serialport adapter sam sie kasowal
        public async Task ConstantReceiveAsync()
        {
            if(_setupSuccessufl)
            {   
                Task.Run(() => TestDataExtractor()).ConfigureAwait(false);
                await Task.Run(() => _device.StartReceivingAsync()).ConfigureAwait(true);
            }
        }

        public async Task<bool> SendAsync(ICommandModel command)
        {
            bool result = await _device.SendAsync(_commandTranslator.FromCommand(command)).ConfigureAwait(false);
            return result;
        }

        // do zrobienia - sprawdzenia? dodac check czy jest open
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
        }
    }
}