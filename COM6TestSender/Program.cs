using Shield.Messaging.Commands;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace COM6TestSender
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            await MainAsync(args);
        }

        public async static Task MainAsync(string[] args)
        {
            SerialPort serial = new SerialPort { BaudRate = /*19200*/921600, Encoding = Encoding.ASCII, PortName = "COM7", DataBits = 8, Parity = Parity.None, StopBits = StopBits.One, ReadTimeout = -1, ParityReplace = 0 };
            serial.DtrEnable = false;
            serial.RtsEnable = false;
            serial.DiscardNull = true;
            serial.Open();

            //IIdGenerator _idGenerator = new IdGeneratorSimplyNext();
            IIdGenerator _idGenerator = new IdGenerator(4);

            int i = 0;

            Console.WriteLine("1 - automat");
            string a = Console.ReadLine();

            Random rand = new Random();
            if (Int32.Parse(a) == 1)
            {
                i++;
                while (true)
                {   //Command(IPart id, IPart hostID, IPart target, IPart order, IPart data)
                    int commandType;
                    do
                    {
                        commandType = rand.Next(1, 16);
                    }
                    while (commandType == 12);

                    string aa = "#" + _idGenerator.GetNewID() + $"*{16.ToString().PadLeft(4, '0')}*" + "target" + '*' + "order" + '*' + i.ToString().PadLeft(30, '.');

                    serial.Write(aa);
                    if (i % 100 == 0) Console.WriteLine(aa);
                    Console.WriteLine(serial.ReadExisting());
                    i++;
                }
            }

            Console.ReadLine();
        }
    }
}