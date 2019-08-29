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

namespace Shield.ConsoleUI
{
    class Application : IApplication
    {
        //private IComPortManager _comPortManager;
        //private IMessageModel _messageModel;
        //private ICommandModel _commamdModel;
        private ICommandModel _command;
        private ISerialPortAdapterFactory _serialPortFactory;
        private IComSender _comSender;
        private IMessanger _comMessanger;
        ICommunicationDeviceFactory _deviceFactory;

        public Application(ICommunicationDeviceFactory deviceFactory, ICommandModel command, ISerialPortAdapterFactory comPortFactory, IComSender comSender, IMessanger comMessanger/*IComPortManager comPortManager, IMessageModel messageModel, ICommandModel commandModel*/)
        {
            //_comPortManager = comPortManager;
            //_messageModel = messageModel;
            //_commamdModel = commandModel;
            _command = command;
            _serialPortFactory = comPortFactory;
            _comSender = comSender;
            _comMessanger = comMessanger;
            _deviceFactory = deviceFactory;
        }
        public void Run()
        {
            //_comPortManager.Create();
            //SerialPort sp = _comPortManager.GetPort();
            //_comPortManager.Clear();
            //sp.Open();
            //Console.WriteLine(sp.IsOpen + " " + sp.PortName);
            //sp.Close();
            //_comPortManager.Create(6);
            //sp = _comPortManager.GetPort();
            //_comPortManager.Clear();
            //sp.Open();
            //Console.WriteLine(sp.IsOpen + " " + sp.PortName);

            //List<string> aa = _comPortManager.AvailablePorts;

            //foreach(string a in aa)
            //{
            //    Console.WriteLine(a);
            //}
            

            //Console.WriteLine(_commamdModel.GetMessage());
            Console.WriteLine( _command.CommandTypeString);

            // dzialajacy serial, do odbioru w putty na com6
            SerialPortAdapter portA = null;
            if (_serialPortFactory.Create(5))
            {
                portA = _serialPortFactory.GivePort;
                //portA.Open();    // Tymczasowo wylaczony do testow commesangera           
            }
            

            // receiver best dependency injection
            ComReceiver receiver = new ComReceiver();
            //receiver.Setup(_serialPortFactory.GivePort, 17);
            //_comSender.Setup(_serialPortFactory.GivePort);

            // na szybko wiadomosc testowa
            CommandModel mes = new CommandModel();
            mes.CommandType = HardwareCom.Enums.CommandType.Data;
            mes.Data = "Test Data inside test command";


            // nie ma takiego portu, do testow
            SerialPortAdapter portB = null;
            if (_serialPortFactory.Create(4))
            {
                portB = _serialPortFactory.GivePort;
                portB.Open();
            }

            // wyswietl porty w kompie
            foreach (var availablePort in _serialPortFactory.AvailablePorts)
            {
                Console.WriteLine(availablePort);
            }

            // test wstepny comMessanger
            SerialPortAdapter portC = null;
            _serialPortFactory.Create(6);
            portC = _serialPortFactory.GivePort;
            portC.Open();
            //portC.Close();
            //portC.Close();
            //_comMessanger.Port = portC;
            //_comMessanger.AddCommandTemp(mes);
            //_comMessanger.Close();
            List<string> aa;

            // test klasy serialportadapter
            //ICommunicationDevice com = new SerialPortAdapter(portA);            
            //com.DataReceived += eve;
            //com.Open();

            System.Diagnostics.Process.GetCurrentProcess().Threads.ToString();
            int licznik = 0;

            Console.WriteLine(System.Diagnostics.Process.GetCurrentProcess().Threads.Count);
            Console.ReadLine();
            // --- petla obiorczo nadawcza do testow!
            while (true)
            {

                portC.Write("aaaa");
                Console.WriteLine(licznik++);
                Console.WriteLine(System.Diagnostics.Process.GetCurrentProcess().Threads.Count);
                //aa = _comMessanger.ReceiveAsync().Result;
                //for(int i = 0 ; i < aa.Count ; i++) 
                //{
                //    Console.WriteLine(aa[i]);                    
                //}
                //aa = null;

                //aa.Clear();
                //string message = Console.ReadLine();                
                //portA.Write(message);
                //_comSender.Command(mes);
                //_comSender.Send();
                //Console.WriteLine(portA.ReadExisting());
                //Console.WriteLine(portA.BytesToRead);
            }

            
            Console.ReadLine();
        }    
        
        void eve(object sender, EventArgs a)
        {
            //Console.WriteLine("Event został odpalony");
        }
    }
}
