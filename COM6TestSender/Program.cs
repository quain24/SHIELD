using Shield.CommonInterfaces;
using Shield.Persistance;
using Shield.HardwareCom.CommandProcessing;
using Shield.HardwareCom.Enums;
using Shield.HardwareCom.Factories;
using Shield.HardwareCom.Helpers;
using Shield.HardwareCom.Models;
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

            Console.WriteLine("1 - automat, 2 - co 1 sekunde, 3 - manual, 4 - notdata, 5 - test random, 6 - misc bad / good,\n7 - partial, 8 - nodata mixed, 9 - Async test, 10 -- message creator, 11 -- Auto message sender - 2 tasks, 12 -- rand msg gen, 13 -- rand 30 msg");
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
                        string aa = $@"*{16.ToString().PadLeft(4, '0')}*" + _idGenerator.GetNewID() + '*' + i.ToString().PadLeft(30, '.');

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
                    string aa = $@"*0016*" + _idGenerator.GetNewID() + '*' + i.ToString().PadLeft(30, '.');
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
                    serial.Write($@"*{"0016"}*" + _idGenerator.GetNewID() + '*' + i.ToString().PadLeft(30, '.'));
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
                    _idGenerator.GetNewID();
                    ii++;
                    if (ii == 100000)
                    {
                        Console.WriteLine("reached 100000");
                        ii = 0;
                        Console.ReadLine();
                    }
                }
            }
            else if (Int32.Parse(a) == 4)
            {
                while (true)
                {
                    serial.Write($@"*0005*" + _idGenerator.GetNewID() + '*');
                    Console.WriteLine(serial.ReadExisting());
                }
            }
            else if (Int32.Parse(a) == 6)
            {
                while (true)
                {
                    i++;
                    string u = $@"*0005*" + _idGenerator.GetNewID() + '*';
                    serial.Write(u);
                    Console.WriteLine(u);

                    u = $@"*0016*" + _idGenerator.GetNewID() + '*' + i.ToString().PadLeft(25, '.');
                    serial.Write(u);
                    Console.WriteLine(u);

                    u = $@"*0006*" + _idGenerator.GetNewID() + '*';
                    serial.Write(u);
                    Console.WriteLine(u);
                    u = $@"*0013*" + _idGenerator.GetNewID() + '*' + i.ToString().PadLeft(30, '.');
                    serial.Write(u);
                    Console.WriteLine(u);
                    u = $@"*0015*" + _idGenerator.GetNewID() + '*' /*+ i.ToString().PadLeft(30, '.')*/;
                    serial.Write(u);
                    Console.WriteLine(u);
                    u = $@"*0001*" + _idGenerator.GetNewID() + '*' + i.ToString().PadLeft(30, '.');
                    serial.Write(u);
                    Console.WriteLine(u);
                    Console.WriteLine(serial.ReadExisting());
                    Console.ReadLine();
                }
            }
            else if (Int32.Parse(a) == 7)
            {
                while (true)
                {
                    i++;
                    string u = $@"*0016*" + _idGenerator.GetNewID() + '*' + i.ToString().PadLeft(20, '.');
                    serial.Write(u);
                    Console.WriteLine(u);
                    Console.ReadLine();

                    u = $@"@#$%^&(()/";
                    serial.Write(u);
                    Console.WriteLine(u);
                    Console.ReadLine();
                }
            }
            else if (Int32.Parse(a) == 8)
            {
                while (true)
                {
                    i++;
                    string u = $@"*0001*" + _idGenerator.GetNewID() + '*';
                    serial.Write(u);
                    Console.WriteLine(u);
                    u = $@"*0002*" + _idGenerator.GetNewID() + '*';
                    serial.Write(u);
                    Console.WriteLine(u);
                    u = $@"*0010*" + _idGenerator.GetNewID() + '*';
                    serial.Write(u);
                    Console.WriteLine(u);
                    u = $@"*0015*12" + _idGenerator.GetNewID() + '*';
                    serial.Write(u);
                    Console.WriteLine(u);
                    u = $@"*0017*" + _idGenerator.GetNewID() + '*';
                    serial.Write(u);
                    Console.WriteLine(u);
                    Console.ReadLine();
                }
            }
            else if (Int32.Parse(a) == 9)
            {
                async Task<bool> boo()
                {
                    bool b = false;
                    while (true)
                    {
                        int aaa = 1;
                        if (aaa > 10)
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
                    string u = $@"*0001*" + _idGenerator.GetNewID() + '*';
                    serial.Write(u);
                    Console.WriteLine(u);
                    u = $@"*0002*" + _idGenerator.GetNewID() + '*';
                    serial.Write(u);
                    Console.WriteLine(u);
                    u = $@"*0010*" + _idGenerator.GetNewID() + '*';
                    serial.Write(u);
                    Console.WriteLine(u);
                    u = $@"*0015*12" + _idGenerator.GetNewID() + '*';
                    serial.Write(u);
                    Console.WriteLine(u);
                    u = $@"*0017*" + _idGenerator.GetNewID() + '*';
                    serial.Write(u);
                    Console.WriteLine(u);
                    Console.ReadLine();
                }
            }
            else if (Int32.Parse(a) == 10)
            {
                int num = 0;

                Console.WriteLine($@"Wybierz typ komenty do wysłania:");
                Console.WriteLine("1 - Handshake");
                Console.WriteLine("2 - Master");
                Console.WriteLine("3 - Slave");
                Console.WriteLine("4 - Confirmation");
                Console.WriteLine("5 - EndMessage");
                Console.WriteLine("6 - ReceivedAsCorrect");
                Console.WriteLine("7 - ReceivedAsError");
                Console.WriteLine("8 - ReceivedAsUnknown");
                Console.WriteLine("9 - ReceivedAsPartial");
                Console.WriteLine("10 - ConfirmationTimeout");
                Console.WriteLine("11 - CompletitionTimeout");
                Console.WriteLine("12 - Error");
                Console.WriteLine("13 - Unknown");
                Console.WriteLine("14 - Partial");
                Console.WriteLine("15 - Confirm");
                Console.WriteLine("16 - Cancel");
                Console.WriteLine("17 - RetryLast");
                Console.WriteLine("18 - Data");
                Console.WriteLine("19 - Renew ID!");

                string pattern = $@"[*][0-9]{{4}}[*][a-zA-Z0-9]{{4}}[*]";

                Regex pat = new Regex(pattern);

                string id = _idGenerator.GetNewID();

                int licz = 0;
                while (true)
                {
                    string data = num.ToString().PadLeft(30, '.');
                    int choose = 0;
                    if (!int.TryParse(Console.ReadLine(), out choose))
                        continue;
                    if (choose == 0 || choose == Int32.MaxValue || choose == Int32.MinValue)
                        continue;
                    Console.WriteLine("OK");

                    string packet = $@"*{choose.ToString().PadLeft(4, '0')}*{id}*";
                    if (choose == 18)
                    {
                        packet += data;
                        num++;
                    }
                    else if (choose == 19)
                    {
                        id = _idGenerator.GetNewID();
                        Console.WriteLine($@"ID changed to {id}");
                        continue;
                    }
                    else if (choose > 17)
                    {
                        Console.WriteLine("bad command");
                        continue;
                    }

                    serial.Write(packet);
                    if (choose == 5)
                    {
                        await Task.Delay(2000);
                        Console.WriteLine("----    RESPONSE    ----");
                        //while (serial.BytesToRead > 0)
                        //{
                        //    string resp = serial.ReadExisting();
                        //    string[] rep = new string[licz+2];
                        //    licz = 0;
                        //    rep = pat.Split(resp);

                        //    foreach(var s in rep)
                        //    {
                        //        Console.WriteLine(s);
                        //    }
                        //}
                        Console.WriteLine(serial.ReadExisting());
                        Console.WriteLine("----  END RESPONSE  ----");
                    }
                }
            }
            else if (Int32.Parse(a) == 11)
            {
                int num = 0;
                int counter = 0;

                string pattern = $@"[*][a-zA-Z0-9]{{4}}[*][0-9]{{4}}[*][a-zA-Z0-9]{{4}}[*]";

                Regex pat = new Regex(pattern);

                string id = _idGenerator.GetNewID();
                List<int> comsetup = new List<int> { 1, 2, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 5, 17 };

                int licz = 0;

                Task.Run(() =>
                {
                    Regex pat2 = new Regex(pattern);

                    string id2 = _idGenerator.GetNewID();
                    List<int> comsetup2 = new List<int> { 1, 3, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 15, 15, 15, 5, 17 };

                    int licz2 = 0;
                    while (true)
                    {
                        foreach (var iii2 in comsetup2)
                        {
                            string data2 = num.ToString().PadLeft(30, '.');
                            int choose2 = iii2;

                            string packet2 = $@"*{choose2.ToString().PadLeft(4, '0')}*{id2}*";
                            if (choose2 == 16)
                            {
                                packet2 += data2;
                                num++;
                            }
                            else if (choose2 == 17)
                            {
                                id2 = _idGenerator.GetNewID();
                                //Console.WriteLine($@"ID changed to {id2}");
                                continue;
                            }
                            else if (choose2 > 17)
                            {
                                Console.WriteLine("bad command");
                                continue;
                            }

                            Console.WriteLine(counter);
                            counter++;
                            serial.Write(packet2);
                            Console.WriteLine(serial.ReadExisting());
                        }
                    }
                });

                while (true)
                {
                    foreach (var iii in comsetup)
                    {
                        string data = num.ToString().PadLeft(30, '.');
                        int choose = iii;

                        string packet = $@"*{choose.ToString().PadLeft(4, '0')}*{id}*";
                        if (choose == 16)
                        {
                            packet += data;
                            num++;
                        }
                        else if (choose == 17)
                        {
                            id = _idGenerator.GetNewID();
                            //Console.WriteLine($@"ID changed to {id}");
                            continue;
                        }
                        else if (choose > 17)
                        {
                            Console.WriteLine("bad command");
                            continue;
                        }

                        //Console.WriteLine(counter);
                        counter++;
                        serial.Write(packet);
                        Console.WriteLine(serial.ReadExisting());
                    }
                }
            }
            else if (Int32.Parse(a) == 12)
            {
                string pattern = $@"[*][a-zA-Z0-9]{{4}}[*][0-9]{{4}}[*][a-zA-Z0-9]{{4}}[*]";
                int num = 0;

                Regex pat = new Regex(pattern);

                string id = _idGenerator.GetNewID();
                int choose = 0;
                int licz = 0;
                bool fl = false;

                var msgFac = new MessageFactory(new Func<IMessageModel>(() => new MessageModel()));
                var comFac = new CommandModelFactory(new Func<ICommandModel>(() => new CommandModel()));

                var sm = new Shield.Persistance.Models.SettingsModel();
                var apset = new Shield.Persistance.Models.ApplicationSettingsModel();
                apset.CommandTypeSize = 4;
                apset.DataSize = 30;
                apset.Filler = '.';
                apset.IdSize = 4;
                apset.HostId = "TEST";
                apset.Separator = '*';
                apset.HostIdSize = 4;
                var portset = new Shield.Persistance.Models.SerialPortSettingsModel();
                portset.BaudRate = 921600;
                portset.Encoding = 20127;
                portset.PortNumber = 7;
                portset.DataBits = 8;
                portset.Parity = Parity.None;
                portset.StopBits = StopBits.One;
                portset.ReadTimeout = -1;

                sm.Settings.Add(Shield.Enums.SettingsType.Application, apset);
                sm.Settings.Add(Shield.Enums.SettingsType.SerialDevice, (ISetting)portset);
                var sett = new Settings(sm);
                var appSet = sett.ForTypeOf<IApplicationSettingsModel>();
                var commandTranslatorSettings = new CommandTranslatorSettings(appSet.Separator, appSet.Filler, appSet.CommandTypeSize, appSet.IdSize, appSet.DataSize, appSet.HostIdSize, apset.HostId);

                var comtrans = new CommandTranslator(commandTranslatorSettings, new CommandModelFactory(new Func<ICommandModel>(() => new CommandModel())));

                while (true)
                {
                    licz++;
                    int datalicz = licz;
                    string data = datalicz.ToString().PadLeft(30, '.');
                    datalicz++;
                    string data2 = datalicz.ToString().PadLeft(30, '.');
                    datalicz++;
                    string data3 = datalicz.ToString().PadLeft(30, '.');

                    IMessageModel msg = msgFac.CreateNew(Direction.Outgoing, MessageType.Master, _idGenerator.GetNewID(), appSet.HostId);
                    msg.Add(comFac.Create(type: CommandType.HandShake, hostId: appSet.HostId));
                    msg.Add(comFac.Create(type: CommandType.Master, hostId: appSet.HostId));
                    var datacom = comFac.Create(type: CommandType.Data, hostId: appSet.HostId); datacom.Data = data;
                    var datacom2 = comFac.Create(type: CommandType.Data, hostId: appSet.HostId); datacom2.Data = data2;
                    var datacom3 = comFac.Create(type: CommandType.Data, hostId: appSet.HostId); datacom3.Data = data3;
                    msg.Add(datacom);
                    msg.Add(datacom2);
                    msg.Add(datacom3);
                    if (licz % 10 != 0)
                        msg.Add(comFac.Create(type: CommandType.EndMessage, hostId: appSet.HostId));

                    foreach (var c in msg.Commands)
                    {
                        var aaaa = comtrans.FromCommand(c);
                        serial.Write(aaaa);
                    }

                    Console.WriteLine($@"{msg.Id} was sent - {msg.CommandCount} commands. Nm: {licz}");
                    //if(licz % 100 == 0)
                    //{
                    //    Console.WriteLine(serial.ReadExisting());
                    //    await Task.Delay(1000);
                    //}
                }
            }
            else if (Int32.Parse(a) == 13)
            {
                string pattern = $@"[*][0-9]{{4}}[*][a-zA-Z0-9]{{4}}[*]";
                int num = 0;

                Regex pat = new Regex(pattern);

                string id = _idGenerator.GetNewID();
                int choose = 0;
                int licz = 0;
                bool fl = false;

                var msgFac = new MessageFactory(new Func<IMessageModel>(() => new MessageModel()));
                var comFac = new CommandModelFactory(new Func<ICommandModel>(() => new CommandModel()));

                var sm = new Shield.Persistance.Models.SettingsModel();
                var apset = new Shield.Persistance.Models.ApplicationSettingsModel();
                apset.CommandTypeSize = 4;
                apset.DataSize = 30;
                apset.Filler = '.';
                apset.IdSize = 4;
                apset.Separator = '*';
                apset.HostIdSize = 4;
                var portset = new Shield.Persistance.Models.SerialPortSettingsModel
                {
                    BaudRate = 921600,
                    Encoding = 20127,
                    PortNumber = 7,
                    DataBits = 8,
                    Parity = Parity.None,
                    StopBits = StopBits.One,
                    ReadTimeout = -1
                };

                var sett = new Settings(sm);
                sm.Settings.Add(Shield.Enums.SettingsType.Application, apset);
                sm.Settings.Add(Shield.Enums.SettingsType.SerialDevice, (ISetting)portset);
                var appSet = sett.ForTypeOf<IApplicationSettingsModel>();
                var commandTranslatorSettings = new CommandTranslatorSettings(appSet.Separator, appSet.Filler, appSet.CommandTypeSize, appSet.IdSize, appSet.DataSize, appSet.HostIdSize, apset.HostId);

                var comtrans = new CommandTranslator(commandTranslatorSettings, new CommandModelFactory(new Func<ICommandModel>(() => new CommandModel())));

            restart:
                choose = licz + 30;

                while (licz <= choose)
                {
                    licz++;
                    int datalicz = licz;
                    string data = datalicz.ToString().PadLeft(30, '.');
                    datalicz++;
                    string data2 = datalicz.ToString().PadLeft(30, '.');
                    datalicz++;
                    string data3 = datalicz.ToString().PadLeft(30, '.');

                    IMessageModel msg = msgFac.CreateNew(Direction.Outgoing, MessageType.Master, appSet.HostId, _idGenerator.GetNewID());
                    msg.Add(comFac.Create(CommandType.HandShake));
                    msg.Add(comFac.Create(CommandType.Master));
                    var datacom = comFac.Create(CommandType.Data); datacom.Data = data;
                    var datacom2 = comFac.Create(CommandType.Data); datacom2.Data = data2;
                    var datacom3 = comFac.Create(CommandType.Data); datacom3.Data = data3;
                    msg.Add(datacom);
                    msg.Add(datacom2);
                    msg.Add(datacom3);
                    if (licz % 10 != 0)
                        msg.Add(comFac.Create(CommandType.EndMessage));

                    foreach (var c in msg.Commands)
                    {
                        var aaaa = comtrans.FromCommand(c);
                        serial.Write(aaaa);
                    }

                    Console.WriteLine($@"{msg.Id} was sent - {msg.CommandCount} commands. Nm: {licz}");
                }
                Console.WriteLine("Repeat for another 30 - press enter");
                Console.ReadLine();
                goto restart;
            }

            // zle na emulatorze
            else
            {
                while (true)
                {
                    string aa = $@"*{16.ToString().PadLeft(4, '0')}*" + _idGenerator.GetNewID() + '*' + i.ToString().PadLeft(30, '.');

                    serial.Write(aa);
                    Console.WriteLine(aa);

                    Console.WriteLine("...");
                    Console.ReadLine();
                    i++;
                }
            }

            Console.ReadLine();
        }
    }
}