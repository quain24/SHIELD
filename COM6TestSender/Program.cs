using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace COM6TestSender
{
    class Program
    {
        static void Main(string[] args)
        {
            SerialPort serial = new SerialPort{ BaudRate = 19200, Encoding = Encoding.ASCII, PortName = "COM6", DataBits = 8, Parity = Parity.None, StopBits = StopBits.One, ReadTimeout = 2000, WriteBufferSize = 2000};
            serial.Open();

            int i = 0;

            Console.WriteLine("Naduś coś by zacząć");
            Console.ReadLine();

            Task.Run(() =>
            {
                while (true)
                {
                    //try
                    //{
                    serial.Write($@"*0001*" + i.ToString().PadLeft(14, '*'));
                    Console.WriteLine(serial.ReadExisting());
                    //}
                    //catch 
                    //{
                    // Console.WriteLine("Nie wysłało");
                    //}
                    i++;

                    //Thread.Sleep(1);


                }
            });


            //while (true)
            //{
            //    //try
            //    //{
            //        serial.Write($@"*0001*" + i.ToString().PadLeft(14, '*'));
            //        Console.WriteLine(serial.ReadExisting());
            //    //}
            //    //catch 
            //    //{
            //        //Console.WriteLine("Nie wysłało");
            //    //}
            //    i++;

            //    //Thread.Sleep(1);


            //}


            Console.WriteLine("Wysyłanie trwa w tasku...");
            Console.ReadLine();







        }
    }
}
