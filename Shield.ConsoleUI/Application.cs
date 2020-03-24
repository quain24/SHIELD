﻿using Shield.Data;
using Shield.Data.Models;
using Shield.Enums;
using Shield.HardwareCom.CommandProcessing;
using Shield.HardwareCom.Factories;
using Shield.HardwareCom.MessageProcessing;
using Shield.HardwareCom.Models;
using Shield.HardwareCom.RawDataProcessing;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading.Tasks;

namespace Shield.ConsoleUI
{
    internal class Application : IApplication
    {
        private ICommandModel _command;
        private ICommunicationDeviceFactory _deviceFactory;
        private IMessageModel _message;
        private ICommandTranslator _commandTranslator;
        private IIncomingDataPreparer _incomingDataPreparer;
        private HardwareCom.IMessenger _comMessanger;
        private ISettings _settings;


        private ICommandIngester _ingester;
        private ICompleteness _completness;

        public Application(ICommunicationDeviceFactory deviceFactory, ICommandModel command, IMessageModel message, ICommandTranslator commandTranslator, IIncomingDataPreparer incomingDataPreparer, HardwareCom.IMessenger messanger, ISettings settings,
            ICommandIngester ingester,
            ICompleteness completness)
        {
            _command = command;
            _deviceFactory = deviceFactory;
            _message = message;
            _commandTranslator = commandTranslator;
            _incomingDataPreparer = incomingDataPreparer;
            _comMessanger = messanger;
            _settings = settings;

            _ingester = ingester;
            _completness = completness;

            //_comcom.IncomingErrorReceived += OnErrorReceived;
        }

        public void Run()
        {
            //ISerialPortSettingsModel settings3 = new SerialPortSettingsModel();
            //settings3.BaudRate = 19200;//921600;//
            //settings3.DataBits = 8;
            //settings3.Parity = Parity.None;
            //settings3.PortNumber = 4;
            //settings3.StopBits = StopBits.One;
            //settings3.ReadTimeout = -1;
            //settings3.WriteTimeout = 100;
            //settings3.Encoding = Encoding.ASCII.CodePage;

            //IMoqPortSettingsModel settings2 = new MoqPortSettingsModel();
            //settings2.PortNumber = 6;

            IApplicationSettingsModel appset = new ApplicationSettingsModel();
            appset.DataSize = 30;
            appset.IdSize = 4;
            appset.CommandTypeSize = 4;
            appset.Filler = '.';
            appset.Separator = '*';

            //_settings.Add(SettingsType.SerialDevice, settings3);
            //_settings.Add(SettingsType.MoqDevice, settings2);
            _settings.AddOrReplace(SettingsType.Application, appset);

            _settings.SaveToFile();
            _settings.LoadFromFile();

            Dictionary<string, IMessageModel> msgcol = new Dictionary<string, IMessageModel>();

            //wyswietl porty w kompie
            foreach (var availablePort in SerialPort.GetPortNames())
            {
                Console.WriteLine(availablePort);
            }

            ////_comMessanger.Setup(DeviceType.Serial);

            //_comcom.AssignMessanger(_comMessanger);

            _comMessanger.Open();
            Task.Run(() => _comMessanger.StartReceiveingAsync());

            //int licznik = 0;

            //Console.WriteLine("Press enter to start sending test messages");

            //Console.ReadLine();

            ////_comMessanger.Setup(DeviceType.Serial);

            //// --- petla obiorczo nadawcza do testow!

            ////_comMessanger.Send(null);

            ////Task.Run(async () =>
            ////{
            ////    while (true)
            ////    {
            ////        CommandModel mes = new CommandModel();
            ////        mes.CommandType = CommandType.Data;
            ////        mes.Data = licznik.ToString();
            ////        mes.Id = Helpers.IdGenerator.GetID(((IApplicationSettingsModel)_setman.GetSettingsFor(SettingsType.Application)).IdSize);

            ////        await _comMessanger.SendAsync(mes);
            ////        licznik++;
            ////    }
            ////});

            //Console.WriteLine("ENTER to close communication line");
            //Console.ReadLine();
            //_comMessanger.StopDecoding();
            ////_comMessanger.Close();
            //Console.WriteLine("ENTER to open communication line");
            //Console.ReadLine();
            //Console.WriteLine("Com line opened.");
            //Task.Run(() => _comMessanger.StartDecodingAsync());
            //Console.WriteLine("ENTER to close communication line");
            //Console.ReadLine();
            //_comMessanger.Close();
            //Console.WriteLine("ENTER to open communication line");
            //Console.ReadLine();
            //Console.WriteLine("Com line opened.");
            //_comMessanger.Open();
            //Task.Run(() => _comMessanger.StartReceiveAsync());
            //Task.Run(() => _comMessanger.StartDecodingAsync());
            //Console.ReadLine();
            //Console.WriteLine("end after enter");
            Console.ReadLine();
        }
    }
}