using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunicationManager;
using CommunicationManager.Utilities;

namespace TemporaryConsoleUI
{
    public class Application : IApplication
    {
        ITestClass _testClass;
        IComPortCommunicator _comPortCommunicator;

        public Application(ITestClass testClass, IComPortCommunicator comPortCommunicator)
        {
            _testClass = testClass;
            _comPortCommunicator = comPortCommunicator;
            
        }
        public void Run()
        {
            _testClass.Test();

            // przypisanie metody do sygnału z portu com6
            _comPortCommunicator.GiveReceiver().DataReceived += test;

            while (true)
            {                               
                 string Data = Console.ReadLine();
                 _comPortCommunicator.SendData(Data);
                Console.WriteLine("Wprowadź kolejne dane");                   
            }
            
        }

        // Odbieram sygnał z portu com6
        public void test(object sender, SerialDataReceivedEventArgs e)
        {
            Console.WriteLine("aaaa");
        }
    }
}
