using Shield.Data;
using Shield.Data.Models;
using Shield.Enums;
using Shield.HardwareCom;
using Shield.HardwareCom.Factories;
using Shield.HardwareCom.Models;
using System;
using System.Diagnostics;
using System.IO.Ports;
using System.Text;
using System.Threading.Tasks;

namespace Shield.ConsoleUI
{
    internal class Application : IApplication
    {
        private ICommandModel _command;
        private ICommunicationDeviceFactory _deviceFactory;
        private IMessanger _comMessanger;
        private IAppSettings _setman;
        private IMessageModel _message;
        private ICommandTranslator _commandTranslator;
        private IIncomingDataPreparer _incomingDataPreparer;

        public Application(IAppSettings setman, ICommunicationDeviceFactory deviceFactory, ICommandModel command, IMessageModel message, ICommandTranslator commandTranslator, IIncomingDataPreparer incomingDataPreparer)
        {
            _command = command;
            _deviceFactory = deviceFactory;
            _setman = setman;
            _message = message;
            _commandTranslator = commandTranslator;
            _incomingDataPreparer = incomingDataPreparer;
        }

        public void Run()
        {
            IApplicationSettingsModel appset = new ApplicationSettingsModel();
            appset.DataSize = 300;
            appset.IdSize = 4;
            appset.CommandTypeSize = 4;
            appset.Filler = '.';
            appset.Separator = '*';

            //// testowanie zapisywania ustawien - działa -

            ISerialPortSettingsModel settings = new SerialPortSettingsModel();
            settings.BaudRate = 921600;//19200;
            settings.DataBits = 8;
            settings.Parity = Parity.None;
            settings.PortNumber = 4;
            settings.StopBits = StopBits.One;
            settings.ReadTimeout = -1;
            settings.WriteTimeout = 100;
            settings.Encoding = Encoding.ASCII.CodePage;

            IMoqPortSettingsModel settings2 = new MoqPortSettingsModel();
            settings2.PortNumber = 6;

            AppSettings setman = new AppSettings(new AppSettingsModel());
            _setman.Add(SettingsType.SerialDevice, settings);
            _setman.Add(SettingsType.MoqDevice, settings2);
            _setman.Add(SettingsType.Application, appset);
            _setman.SaveToFile();

            //foreach (var item in _setman.GetAll())
            //{
            //    Console.WriteLine("Pozycja sprzed wczytania:");
            //    Console.WriteLine(item.Key.ToString() + " " + item.Value.ToString());
            //}

            // wczytywanie ustawien
            _setman.LoadFromFile();

            //// Prymityne wyswietlanie wartosci
            //foreach (var item in _setman.GetAll())
            //{
            //    if(item.Key == SettingsType.MoqDevice)
            //    {
            //        Console.WriteLine("Pozycja po wczytaniu:");
            //        Console.WriteLine("klucz: " + item.Key.ToString());
            //        IMoqPortSettingsModel readed2 = (IMoqPortSettingsModel) item.Value;
            //        Console.WriteLine(readed2.PortNumber);
            //    }
            //    else if(item.Key == SettingsType.Application)
            //    {
            //        Console.WriteLine("Pozycja po wczytaniu:");
            //        Console.WriteLine("klucz: " + item.Key.ToString());
            //        IApplicationSettingsModel readed3 = (IApplicationSettingsModel) item.Value;
            //        Console.WriteLine(readed3.DataSize);
            //    }
            //    else
            //    {
            //        Console.WriteLine("Pozycja po wczytaniu:");
            //        Console.WriteLine("klucz: " + item.Key.ToString());
            //        ISerialPortSettingsModel readed = (ISerialPortSettingsModel) item.Value;
            //        Console.WriteLine(readed.BaudRate);
            //        Console.WriteLine(readed.DataBits);
            //        Console.WriteLine(readed.Parity);
            //        Console.WriteLine(readed.PortNumber);
            //        Console.WriteLine(readed.StopBits);
            //        Console.WriteLine(readed.ReadTimeout);
            //        Console.WriteLine(readed.WriteTimeout);
            //        Console.WriteLine(readed.Encoding.ToString());
            //    }
            //}

            _comMessanger = new Messanger(_setman, _deviceFactory, _commandTranslator, _incomingDataPreparer);
            _comMessanger.Setup(DeviceType.Serial);

            // wyswietl porty w kompie
            foreach (var availablePort in SerialPort.GetPortNames())
            {
                Console.WriteLine(availablePort);
            }

            _comMessanger.Open();
            _comMessanger.ConstantReceiveAsync();

            int licznik = 0;

            Console.WriteLine(Process.GetCurrentProcess().Threads.Count);

            Console.WriteLine("Press enter to send test message");
            Console.ReadLine();
            Console.WriteLine("Send a test IMessage model");
            ICommandModel com1 = new CommandModel { CommandType = CommandType.HandShake };
            ICommandModel com2 = new CommandModel { CommandType = CommandType.Sending };
            ICommandModel com3 = new CommandModel { CommandType = CommandType.Data, Data = "123456789123456789123456789123" };
            ICommandModel com4 = new CommandModel { CommandType = CommandType.Completed };

            IMessageModel message = new MessageModel(_setman);
            message.Add(com1);
            message.Add(com2);
            message.Add(com3);
            message.Add(com4);

            _comMessanger.SendAsync(message);

            Console.WriteLine("Message sent.");

            Console.ReadLine();

            //_comMessanger.Setup(DeviceType.Serial);

            // --- petla obiorczo nadawcza do testow!

            //_comMessanger.Send(null);

            Task.Run(async () =>
            {
                while (true)
                {
                    CommandModel mes = new CommandModel();
                    mes.CommandType = CommandType.Data;
                    mes.Data = licznik.ToString();
                    mes.Id = Helpers.IdGenerator.GetId(((IApplicationSettingsModel)_setman.GetSettingsFor(SettingsType.Application)).IdSize);

                    await _comMessanger.SendAsync(mes);
                    //await Task.Delay(500);
                    //    await Task.Delay(100);
                    //if (licznik % 1000 == 0)
                    //  GC.Collect();
                    //Thread.Sleep(1);
                    //await Task.Delay(1);

                    licznik++;
                }
            });

            //Task.Run(async () =>
            //{
            //    while (true)
            //    {
            //        foreach (var command in messageList)
            //        {
            //            if (!_comMessanger.Send(command))
            //                await Task.Delay(100);
            //        }
            //    }
            //});

            //while (true)
            //{
            //    //Task.Run(() => _comMessanger.Send(mes));    // To nam pomaga w wysylaniu - koniec exception timeout wspomnianych w serialportadapter - zobaczyc!

            //    //_comMessanger.Send(mes);
            //    //Console.WriteLine(licznik++);
            //    //licznik++;
            //    //mes = new CommandModel();
            //    //mes.CommandType = CommandType.Data;
            //    //mes.Data = licznik.ToString();
            //    //_comMessanger.Send(mes);
            //    //Thread.Sleep(1);       // Spowolnienie daje oddech i pozwala na wyswietlenie wiadomosci odbiorczej - inaczej przeciez glowny watek zapcha wszystko!
            //    if(licznik > 10000)
            //        {
            //        Thread.Sleep(5000);
            //        licznik = 0;
            //    }

            //}

            // o wiele bardziej zapycha niz powyzsze w task run - dlaczego? odpala mase gc
            //while (licznik < 80000)
            //{
            //    mes = new CommandModel();
            //    mes.CommandType = CommandType.Data;
            //    mes.Data = licznik.ToString();
            //    _comMessanger.SendAsync(mes);
            //    //if (licznik % 1000 == 0)
            //    //GC.Collect();
            //    //Thread.Sleep(1);
            //    //await Task.Delay(1);

            //    licznik++;
            //}

            // _comMessanger.Close();
            Console.WriteLine("Waiting for signal...");
            //Thread.Sleep(5000);
            //_comMessanger.Close();
            Console.ReadLine();
            _comMessanger.Close();
            Console.ReadLine();
        }
    }
}