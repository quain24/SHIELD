using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shield.HardwareCom.Models;
using Shield.HardwareCom.Factories;
using Shield.HardwareCom;

namespace Shield.ConsoleUI
{
    class Application : IApplication
    {
        //private IComPortManager _comPortManager;
        //private IMessageModel _messageModel;
        //private ICommandModel _commamdModel;
        private ICommand _command;
        private IComPortFactory _comPortFactory;
        private IComSender _comSender;

        public Application(ICommand command, IComPortFactory comPortFactory, IComSender comSender/*IComPortManager comPortManager, IMessageModel messageModel, ICommandModel commandModel*/)
        {
            //_comPortManager = comPortManager;
            //_messageModel = messageModel;
            //_commamdModel = commandModel;
            _command = command;
            _comPortFactory = comPortFactory;
            _comSender = comSender;
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
            SerialPort portA = null;
            if (_comPortFactory.Create(5))
            {
                portA = _comPortFactory.GivePort;
                portA.Open();               
            }
            

            // receiver best dependency injection
            ComReceiver receiver = new ComReceiver();
            receiver.Setup(_comPortFactory.GivePort);
            _comSender.Setup(_comPortFactory.GivePort);

            // na szybko wiadomosc testowa
            Command mes = new Command();
            mes.CommandType = HardwareCom.Enums.CommandType.Data;
            mes.Data = "Test Data inside test command";


            // nie ma takiego portu, do testow
            SerialPort portB = null;
            if (_comPortFactory.Create(4))
            {
                portB = _comPortFactory.GivePort;
                portB.Open();
            }

            // wyswietl porty w kompie
            foreach (var availiblePort in _comPortFactory.AvailablePorts)
            {
                Console.WriteLine(availiblePort);
            }           

            //string a = receiver.RceiveAsync();

            // --- petla obiorczo nadawcza do testow!
            while (true)
            {
                string message = Console.ReadLine();                
                //portA.Write(message);
                _comSender.Message(mes);
                _comSender.Send();
                //Console.WriteLine(portA.ReadExisting());
                Console.WriteLine(portA.BytesToRead);
            }


            Console.ReadLine();
        }       
    }
}
