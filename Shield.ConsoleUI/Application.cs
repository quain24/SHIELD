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
using System.Threading;

namespace Shield.ConsoleUI
{
    class Application : IApplication
    {
        private ICommandModel _command;
        private ICommunicationDeviceFactory _deviceFactory;
        private IMessanger _comMessanger;
        private IAppSettings _setman;

        public Application(IAppSettings setman, ICommunicationDeviceFactory deviceFactory, ICommandModel command)
        {           
            _command = command;           
            _deviceFactory = deviceFactory;
            _setman = setman;
        }
        public void Run()
        {
            IApplicationSettingsModel appset = new ApplicationSettingsModel();
            appset.MessageSize = 20;


            // testowanie zapisywania ustawien - działa - 

            ISerialPortSettingsModel settings = new SerialPortSettingsModel();
            settings.BaudRate = 19200;
            settings.DataBits = 8;
            settings.Parity = Parity.None;
            settings.PortNumber = 5;
            settings.StopBits = StopBits.One;
            settings.ReadTimeout = 2000;
            settings.WriteTimeout = 2000;
            settings.Encoding = Encoding.ASCII.CodePage;

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

            // wczytywanie ustawien
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
                    Console.WriteLine(readed.ReadTimeout);
                    Console.WriteLine(readed.WriteTimeout);
                    Console.WriteLine(readed.Encoding.ToString());
                }                
            }
            
            // ICommunicationDevice portXX = _deviceFactory.Device(DeviceType.serial, 5); - przeniesione do messangera
            //ICommunicationDevice portYY = _deviceFactory.Device(DeviceType.Serial, 6, 20);
            
            //portYY.Open();

            //ICommunicationDevice portZZ = _deviceFactory.Device(DeviceType.Serial);

            _comMessanger = new Messanger(_setman, _deviceFactory);
            _comMessanger.Setup(DeviceType.Serial);

            // na szybko wiadomosc testowa 

            CommandModel mes = new CommandModel();
            mes.CommandType = CommandType.Data;
            mes.Data = "aaaaaaaaaaaaaa"; 

            // wyswietl porty w kompie
            foreach (var availablePort in SerialPort.GetPortNames())
            {
                Console.WriteLine(availablePort);
            }   
           
            
            Process.GetCurrentProcess().Threads.ToString();
            int licznik = 0;

            Console.WriteLine(Process.GetCurrentProcess().Threads.Count);



            Console.ReadLine();
            //_comMessanger.Setup(DeviceType.Serial);

            // --- petla obiorczo nadawcza do testow!

            //_comMessanger.Send(null);



            //Task.Run(async () =>
            //{
            //    while (true)
            //    {
            //        _comMessanger.Send(mes);
            //        await Task.Delay(1);
            //        //Thread.Sleep(10);
            //        licznik++;
            //    }
            //});

            _comMessanger.Open();


            while (true)
            {
                //Task.Run(() => _comMessanger.Send(mes));    // To nam pomaga w wysylaniu - koniec exception timeout wspomnianych w serialportadapter - zobaczyc!

                //_comMessanger.Send(mes);
                Console.WriteLine(licznik++);
                Thread.Sleep(10);       // Spowolnienie daje oddech i pozwala na wyswietlenie wiadomosci odbiorczej - inaczej przeciez glowny watek zapcha wszystko!
                if(licznik > 100000)
                    break;


            }
            _comMessanger.Close();
            Console.WriteLine("Waiting for signal...");
            Console.ReadLine();
            
        }

        void eve(object sender, EventArgs a)
        {
            //Console.WriteLine("Event został odpalony");
        }
    }
}
