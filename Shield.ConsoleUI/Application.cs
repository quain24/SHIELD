using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shield.HardwareCom.Models;
using Shield.HardwareCom.Factories;
using Shield.HardwareCom;
using Shield.HardwareCom.CommonInterfaces;
using Shield.HardwareCom.Adapters;
using System.Diagnostics;
using Shield.Enums;

namespace Shield.ConsoleUI
{
    class Application : IApplication
    {
        //private IComPortManager _comPortManager;
        //private IMessageModel _messageModel;
        //private ICommandModel _commamdModel;
        private ICommandModel _command;
        private ICommunicationDeviceFactory _deviceFactory;
        private IMessanger _comMessanger;
        private IComSender _comSender;
        private IComReceiver _comReceiver;

        public Application(ICommunicationDeviceFactory deviceFactory, ICommandModel command, IComSender comSender, IComReceiver comReceiver)
        {
           
            _command = command;           
            _deviceFactory = deviceFactory;
            _comSender = comSender;
            _comReceiver = comReceiver;
        }
        public void Run()
        {

            // ICommunicationDevice portXX = _deviceFactory.Device(DeviceType.serial, 5); - przeniesione do messangera
            ICommunicationDevice portYY = _deviceFactory.Device(DeviceType.serial, 6);
            portYY.Open();

            _comMessanger = new Messanger(_deviceFactory, _comSender, _comReceiver);
            _comMessanger.Setup(DeviceType.serial, 5);

            // na szybko wiadomosc testowa
            CommandModel mes = new CommandModel();
            mes.CommandType = CommandType.Data;
            mes.Data = "Test Data inside test command";            

            // wyswietl porty w kompie
            foreach (var availablePort in SerialPort.GetPortNames())
            {
                Console.WriteLine(availablePort);
            }   
           
            
            System.Diagnostics.Process.GetCurrentProcess().Threads.ToString();
            int licznik = 0;

            Console.WriteLine(System.Diagnostics.Process.GetCurrentProcess().Threads.Count);
            Console.ReadLine();
            // --- petla obiorczo nadawcza do testow!
            while (true)
            {
                _comMessanger.Send(mes);
                Console.WriteLine(portYY.Read());
                
                Console.WriteLine(licznik++);
                Console.WriteLine(System.Diagnostics.Process.GetCurrentProcess().Threads.Count);
                
            }
        }    
        
        void eve(object sender, EventArgs a)
        {
            //Console.WriteLine("Event został odpalony");
        }
    }
}
