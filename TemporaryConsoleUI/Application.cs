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

        public Application(IComPortManager comPortManager, IMessageModel messageModel )
        {
            _comPortManager = comPortManager;
            _messageModel = messageModel;
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

            CommandModel command = new CommandModel("dddd");
            _messageModel = new MessageModel(command);
            Console.WriteLine(_messageModel.GetACommand());
            

            Console.ReadLine();
        }

        // Odbieram sygnał z portu com6
        public void test(object sender, SerialDataReceivedEventArgs e)
        {
            Console.WriteLine("aaaa");
        }
    }
}
