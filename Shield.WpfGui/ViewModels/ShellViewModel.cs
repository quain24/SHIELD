using Shield.HardwareCom.Adapters;
using Shield.CommonInterfaces;
using Shield.HardwareCom.Factories;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Shield.WpfGui.ViewModels
{
  

    public class ShellViewModel
    {
        SerialPortAdapter portC;
        SerialPortAdapter portA;
        int licznik = 0;
        public void zacznij()
        {
            //ISerialPortAdapterFactory _comPortFactory = new SerialPortAdapterFactory();
            //portA = null;
            //if (_comPortFactory.Create(5))
            //{
            //    portA = _comPortFactory.GivePort;
            //    //portA.Open();    // Tymczasowo wylaczony do testow commesangera           
            //}

            //ICommunicationDevice com = portA;            
            //com.DataReceived += eve;
            //com.Open();

            //portC = null;
            //_comPortFactory.Create(6);
            //portC = _comPortFactory.GivePort;
            //portC.Open();

            //Task.Run( () => {
            //    while (true)
            //    {
            //        portC.Write("aaaa");
            //        //Thread.Sleep(5);
            //    }
            //    }
            //);
            

            //Debug.WriteLine("Rozpoczęto");
        }
        //testy watkow - czy zawieszaja
       public void but()
        {
            Debug.WriteLine("kliknieto");
             while (true)
            {

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
        }




        void eve(object sender, EventArgs a)
        {
            licznik++;
            //Console.WriteLine("Event został odpalony");
            //Debug.WriteLine("Odpalony event w wątku pobocznym?");
             Debug.WriteLine(licznik);
            portA.DiscardInBuffer();
        }
    }
}
