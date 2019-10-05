using System;
using System.IO.Ports;
using System.Text;
using System.Threading.Tasks;

namespace COM6TestSender
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            SerialPort serial = new SerialPort { BaudRate = 921600, Encoding = Encoding.ASCII, PortName = "COM7", DataBits = 8, Parity = Parity.None, StopBits = StopBits.One, ReadTimeout = -1, ParityReplace = 0 };
            serial.DtrEnable = false;
            serial.RtsEnable = false;
            serial.DiscardNull = true;
            serial.Open();

            int i = 0;

            Console.WriteLine("1 - automat, 2 - co 1 sekunde, 3 - manual, 4 - foreach");
            string a = Console.ReadLine();

            Random rand = new Random();
            if (Int32.Parse(a) == 1)
            {
                Task.Run(() =>
                {
                    i++;
                    while (true)
                    {
                        //try
                        //{
                        int commandType;
                        do
                        {
                            commandType = rand.Next(1, 16);
                        }
                        while (commandType == 12);

                        //Thread.Sleep(10);
                        //serial.Write($@"*0015*ABCD*123456789101112131415161718192");
                        //serial.Write($@"*{15.ToString().PadLeft(4, '0')}*" + rand.Next(1000, 9999) + '*' + "A1B2C3D4E5F6G7H8I9J10K11L12M13");
                        string aa = $@"*{15.ToString().PadLeft(4, '0')}*" + Shield.Helpers.IdGenerator.GetId(4) + '*' + i.ToString().PadLeft(30, '.');

                        serial.Write(aa);
                        Console.WriteLine(aa);
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
            }
            else if (Int32.Parse(a) == 2)
            {
                Task.Run(async () =>
            {
                while (true)
                {
                    //try
                    //{
                    string aa = $@"*0015*" + Shield.Helpers.IdGenerator.GetId(4) + '*' + i.ToString().PadLeft(30, '.');
                    byte[] bak = new byte[aa.Length];
                    bak = Encoding.ASCII.GetBytes(aa);
                    //Thread.Sleep(1);
                    await serial.BaseStream.WriteAsync(bak, 0, 41);

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
            }

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
            else if (Int32.Parse(a) == 3)
            {
                while (true)
                {
                    serial.Write($@"*{"0015"}*" + i.ToString().PadLeft(4, '0') + '*' + i.ToString().PadLeft(30, '.'));
                    Console.WriteLine(serial.ReadExisting());

                    i++;
                    Console.WriteLine("...");
                    Console.ReadLine();
                }
            }

            // zle na emulatorze
            else
            {
                while (true)
                {
                    string aa = $@"*0015*" + Shield.Helpers.IdGenerator.GetId(4) + '*' + i.ToString().PadLeft(30, '.');
                    serial.Write(aa);
                    serial.DiscardOutBuffer();
                    Console.WriteLine(aa);

                    Console.WriteLine("...");
                    Console.ReadLine();
                    i++;
                }
            }

            Console.WriteLine("Wysyłanie trwa w tasku...");

            Console.ReadLine();
        }
    }
}