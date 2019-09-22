using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            SerialPort serial = new SerialPort{ BaudRate = 19200, Encoding = Encoding.ASCII, PortName = "COM4", DataBits = 8, Parity = Parity.None, StopBits = StopBits.One, ReadTimeout = -1, ParityReplace = 0};
            serial.DtrEnable = false;
            serial.RtsEnable = false;
            serial.DiscardNull = true;
            serial.Open();

            int i = 0;

            Console.WriteLine("Testowy odbiór poprzez stream - naduś enter by rozpocząć");

            string a = Console.ReadLine();

            Random rand = new Random();

                        
            Task.Run(async () =>
            {
                byte[] mainBuffer = new byte[41];
                Console.WriteLine("Odb. rozpoczęty");
                
                
                StringBuilder buffer = new StringBuilder();

                
                while (true)
                {
                    int bytesRead = await serial.BaseStream.ReadAsync(mainBuffer, 0, 41); 
                    i++;

                    buffer.Append(Encoding.ASCII.GetString(mainBuffer).Substring(0, bytesRead));

                    if(buffer.Length >= 41)
                    {
                        Console.WriteLine(buffer.ToString().Substring(0, 41));
                        buffer.Remove(0, 41);
                    }

                }
            });
            Console.ReadLine();
        }
    }
}
