using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunicationManager;
using CommunicationManager.Models;

namespace TemporaryConsoleUI
{
    public class Application : IApplication
    {
        private IComPortManager _comPortManager;
        private IMessageModel _messageModel;
        private ICommandModel _commamdModel;

        public Application(IComPortManager comPortManager, IMessageModel messageModel, ICommandModel commandModel)
        {
            _comPortManager = comPortManager;
            _messageModel = messageModel;
            _commamdModel = commandModel;
        }
        public void Run()
        {
            _comPortManager.Create();
            SerialPort sp = _comPortManager.GetPort();
            _comPortManager.Clear();
            sp.Open();
            Console.WriteLine(sp.IsOpen + " " + sp.PortName);
            sp.Close();
            _comPortManager.Create(6);
            sp = _comPortManager.GetPort();
            _comPortManager.Clear();
            sp.Open();
            Console.WriteLine(sp.IsOpen + " " + sp.PortName);

            List<string> aa = _comPortManager.AvailablePorts;

            foreach(string a in aa)
            {
                Console.WriteLine(a);
            }


            Console.WriteLine(_commamdModel.GetMessage());
            

            Console.ReadLine();
        }       
    }
}
