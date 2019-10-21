using System;
using System.IO.Ports;
using System.Text;
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



            SerialPort serial = new SerialPort { BaudRate = 921600, Encoding = Encoding.ASCII, PortName = "COM7", DataBits = 8, Parity = Parity.None, StopBits = StopBits.One, ReadTimeout = -1, ParityReplace = 0 };
            serial.DtrEnable = false;
            serial.RtsEnable = false;
            serial.DiscardNull = true;
            serial.Open();

            int i = 0;

            Console.WriteLine("1 - automat, 2 - co 1 sekunde, 3 - manual, 4 - notdata, 5 - test random, 6 - misc bad / good, 7 - partial, 8 - nodata mixed, 9 - Async test");
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
                        string aa = $@"*{13.ToString().PadLeft(4, '0')}*" + Shield.Helpers.IdGenerator.GetId(4) + '*' + i.ToString().PadLeft(30, '.');

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
                await Task.Run(async () =>
            {
                while (true)
                {
                    //try
                    //{
                    string aa = $@"*0013*" + Shield.Helpers.IdGenerator.GetId(4) + '*' + i.ToString().PadLeft(30, '.');
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
                    serial.Write($@"*{"0013"}*" + Shield.Helpers.IdGenerator.GetId(4) + '*' + i.ToString().PadLeft(30, '.'));
                    Console.WriteLine(serial.ReadExisting());

                    i++;
                    Console.WriteLine("...");
                    Console.ReadLine();
                }
            }
            else if (Int32.Parse(a) == 5)
            {
                int ii = 0;
                while (true)
                {
                    Shield.Helpers.IdGenerator.GetId(6);
                    ii++;
                    if (ii == 100000)
                    {
                        Console.WriteLine("reached 100000");
                        ii = 0;
                        Console.ReadLine();
                    }
                }
            }

            else if(Int32.Parse(a) == 4)
            {
                while (true)
                {
                    serial.Write($@"*0005*" + Shield.Helpers.IdGenerator.GetId(4) + '*');
                    Console.WriteLine(serial.ReadExisting());

                }

            }

            else if(Int32.Parse(a) == 6)
            {
                while (true)
                {
                    i++;
                    string u = $@"*0005*" + Shield.Helpers.IdGenerator.GetId(4) + '*';
                    serial.Write(u);
                    Console.WriteLine(u);

                    u = $@"*0013*" + Shield.Helpers.IdGenerator.GetId(4) + '*' + i.ToString().PadLeft(25, '.');
                    serial.Write(u);
                    Console.WriteLine(u);

                    u = $@"*0006*" + Shield.Helpers.IdGenerator.GetId(4) + '*';
                    serial.Write(u);
                    Console.WriteLine(u);
                    u = $@"*0013*" + Shield.Helpers.IdGenerator.GetId(4) + '*' + i.ToString().PadLeft(30, '.');
                    serial.Write(u);
                    Console.WriteLine(u);
                    u = $@"*0015*" + Shield.Helpers.IdGenerator.GetId(4) + '*' /*+ i.ToString().PadLeft(30, '.')*/;
                    serial.Write(u);
                    Console.WriteLine(u);
                    u = $@"*0001*" + Shield.Helpers.IdGenerator.GetId(4) + '*' + i.ToString().PadLeft(30, '.');
                    serial.Write(u);
                    Console.WriteLine(u);
                    Console.WriteLine(serial.ReadExisting());
                    Console.ReadLine();

                }

            }
            else if(Int32.Parse(a) == 7)
            {
                while (true)
                {
                    i++;
                    string u = $@"*0013*" + Shield.Helpers.IdGenerator.GetId(4) + '*' + i.ToString().PadLeft(20, '.');
                    serial.Write(u);
                    Console.WriteLine(u);
                    Console.ReadLine();


                    u = $@"@#$%^&(()/";
                    serial.Write(u);
                    Console.WriteLine(u);
                    Console.ReadLine();

                }

            }

            else if(Int32.Parse(a) == 8)
            {
                while (true)
                {
                    i++;
                    string u = $@"*0001*"  + Shield.Helpers.IdGenerator.GetId(4) + '*';
                    serial.Write(u);
                    Console.WriteLine(u);
                     u = $@"*0002*" + Shield.Helpers.IdGenerator.GetId(4) + '*';
                    serial.Write(u);
                    Console.WriteLine(u);
                     u = $@"*0010*" + Shield.Helpers.IdGenerator.GetId(4) + '*';
                    serial.Write(u);
                    Console.WriteLine(u);
                     u = $@"*0015*12" + Shield.Helpers.IdGenerator.GetId(4) + '*';
                    serial.Write(u);
                    Console.WriteLine(u);
                     u = $@"*0016*" + Shield.Helpers.IdGenerator.GetId(4) + '*';
                    serial.Write(u);
                    Console.WriteLine(u);
                    Console.ReadLine();                    

                }

            }

            else if(Int32.Parse(a) == 9)
            {
                async Task<bool> boo()
                {
                    bool b = false;
                    while (true)
                    {
                        int aaa = 1;
                        if(aaa > 10)
                            break;
                    }

                    return b;
                }

                Console.WriteLine("Starting");
                Console.WriteLine("running task");
                
                var test1 = boo().ConfigureAwait(false);
                Console.ReadLine();
                Console.WriteLine("Task Assigned");
                await test1;
                Console.WriteLine("Starting Awaiting");
                Console.WriteLine("Post await");





                while (true)
                {
                    i++;
                    string u = $@"*0001*"  + Shield.Helpers.IdGenerator.GetId(4) + '*';
                    serial.Write(u);
                    Console.WriteLine(u);
                     u = $@"*0002*" + Shield.Helpers.IdGenerator.GetId(4) + '*';
                    serial.Write(u);
                    Console.WriteLine(u);
                     u = $@"*0010*" + Shield.Helpers.IdGenerator.GetId(4) + '*';
                    serial.Write(u);
                    Console.WriteLine(u);
                     u = $@"*0015*12" + Shield.Helpers.IdGenerator.GetId(4) + '*';
                    serial.Write(u);
                    Console.WriteLine(u);
                     u = $@"*0016*" + Shield.Helpers.IdGenerator.GetId(4) + '*';
                    serial.Write(u);
                    Console.WriteLine(u);
                    Console.ReadLine();                    

                }

            }

            // zle na emulatorze
            else
            {
                while (true)
                {
                    string aa = $@"*{13.ToString().PadLeft(4, '0')}*" + Shield.Helpers.IdGenerator.GetId(4) + '*' + i.ToString().PadLeft(30, '.');

                    serial.Write(aa);
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