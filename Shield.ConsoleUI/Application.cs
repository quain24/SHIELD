using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shield.HardwareCom.Models;
using Shield.HardwareCom.Factories;
using Shield.HardwareCom;
using Shield.CommonInterfaces;
using System.Diagnostics;
using Shield.Enums;
using Shield.Data;
using Shield.Data.Models;

namespace Shield.ConsoleUI
{
    class Application : IApplication
    {
        private ICommandModel _command;
        private ICommunicationDeviceFactory _deviceFactory;
        private IMessanger _comMessanger;
        private IComSender _comSender;
        private IComReceiver _comReceiver;
        private IAppSettings _setman;

        public Application(IAppSettings setman, ICommunicationDeviceFactory deviceFactory, ICommandModel command, IComSender comSender, IComReceiver comReceiver)
        {           
            _command = command;           
            _deviceFactory = deviceFactory;
            _comSender = comSender;
            _comReceiver = comReceiver;
            _setman = setman;
        }
        public void Run()
        {
            IApplicationSettingsModel appset = new ApplicationSettingsModel();
            appset.MessageSize = 20;


            // testowanie zapisywania i wczytywania ustawien - działa - tymczaasowo wygaszono, tylko wczytuje z pliku - automat

            ISerialPortSettingsModel settings = new SerialPortSettingsModel();
            settings.BaudRate = 19200;
            settings.DataBits = 6;
            settings.Parity = Parity.None;
            settings.PortNumber = 5;
            settings.StopBits = StopBits.One;
            settings.CommandSize = 20;

            IMoqPortSettingsModel settings2 = new MoqPortSettingsModel();
            settings2.PortNumber = 6;

            //AppSettings setman = new AppSettings();
            _setman.Add(SettingsType.SerialDevice, settings);
            _setman.Add(SettingsType.MoqDevice, settings2);
            _setman.Add(SettingsType.Application, appset);
            _setman.SaveToFile();
            
            foreach (var item in _setman.GetAll())
            {
                Console.WriteLine("Pozycja sprzed wczytania:");
                Console.WriteLine(item.Key.ToString() + " " + item.Value.ToString());
            }

            _setman.LoadFromFile();
            
            // Prymityne wyswietlanie wartosci
            foreach (var item in _setman.GetAll())
            {
                if(item.Key == SettingsType.MoqDevice)
                {
                    Console.WriteLine("Pozycja po wczytaniu:");
                    Console.WriteLine("klucz: " + item.Key.ToString());
                    IMoqPortSettingsModel readed2 = (IMoqPortSettingsModel) item.Value;
                    Console.WriteLine(readed2.PortNumber);
                }
                else if(item.Key == SettingsType.Application)
                {
                    Console.WriteLine("Pozycja po wczytaniu:");
                    Console.WriteLine("klucz: " + item.Key.ToString());
                    IApplicationSettingsModel readed3 = (IApplicationSettingsModel) item.Value;
                    Console.WriteLine(readed3.MessageSize);
                }
                else
                {
                    Console.WriteLine("Pozycja po wczytaniu:");
                    Console.WriteLine("klucz: " + item.Key.ToString());
                    ISerialPortSettingsModel readed = (ISerialPortSettingsModel) item.Value;
                    Console.WriteLine(readed.BaudRate);
                    Console.WriteLine(readed.DataBits);
                    Console.WriteLine(readed.Parity);
                    Console.WriteLine(readed.PortNumber);
                    Console.WriteLine(readed.StopBits);
                }                
            }
            
            // ICommunicationDevice portXX = _deviceFactory.Device(DeviceType.serial, 5); - przeniesione do messangera
            ICommunicationDevice portYY = _deviceFactory.Device(DeviceType.Serial, 6, 20);
            portYY.Open();

            ICommunicationDevice portZZ = _deviceFactory.Device(DeviceType.Serial);

            _comMessanger = new Messanger(_setman, _deviceFactory);
            _comMessanger.Setup(DeviceType.Serial);

            // na szybko wiadomosc testowa
            CommandModel mes = new CommandModel();
            mes.CommandType = CommandType.Data;
            mes.Data = "Test Data inside test command";            

            // wyswietl porty w kompie
            foreach (var availablePort in SerialPort.GetPortNames())
            {
                Console.WriteLine(availablePort);
            }   
           
            
            Process.GetCurrentProcess().Threads.ToString();
            int licznik = 0;

            Console.WriteLine(Process.GetCurrentProcess().Threads.Count);
            Console.ReadLine();
            _comMessanger.Setup(DeviceType.Serial);
           
            // --- petla obiorczo nadawcza do testow!
            while (true)
            {
                Console.WriteLine(  _comMessanger.GetBuf);
                _comMessanger.Send(mes);
                Console.WriteLine(portYY.Receive());
                
                Console.WriteLine(licznik++);
                Console.WriteLine(Process.GetCurrentProcess().Threads.Count);
                
            }
        }    
        
        void eve(object sender, EventArgs a)
        {
            //Console.WriteLine("Event został odpalony");
        }
    }
}
