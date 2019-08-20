using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunicationManager;

namespace TemporaryConsoleUI
{
    public class Application : IApplication
    {
        private IComPortManager _comPortManager;

        public Application(IComPortManager comPortManager )
        {
            _comPortManager = comPortManager;                
        }
        public void Run()
        {
            _comPortManager.Create();
            SerialPort sp = _comPortManager.GetPort();
            _comPortManager.Clear();
            sp.Open();
            Console.WriteLine(sp.IsOpen);
            
            Console.ReadLine();
        }

        // Odbieram sygnał z portu com6
        public void test(object sender, SerialDataReceivedEventArgs e)
        {
            Console.WriteLine("aaaa");
        }
    }
}
