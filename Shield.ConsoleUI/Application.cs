using Shield.Data;
using Shield.Data.Models;
using Shield.Enums;
using Shield.HardwareCom;
using Shield.HardwareCom.Factories;
using Shield.HardwareCom.Models;
using System;
using System.IO.Ports;
using System.Text;
using System.Threading.Tasks;


namespace Shield.ConsoleUI
{
    internal class Application : IApplication
    {
        private ICommandModel _command;
        private ICommunicationDeviceFactory _deviceFactory;
        private IMessageModel _message;
        private ICommandTranslator _commandTranslator;
        private IIncomingDataPreparer _incomingDataPreparer;
        private IMessanger _comMessanger;
        private ICommandTypesModel _commandTypesModel;
        private ISettings _settings;
        //private ComCommander _comcom = new ComCommander(new CommandModelFactory(new Func<ICommandModel>(() => { return new CommandModel(); })), new Func<IMessageModel>(() => { return new MessageModel(); }));

        public Application(ICommunicationDeviceFactory deviceFactory, ICommandModel command, IMessageModel message, ICommandTranslator commandTranslator, IIncomingDataPreparer incomingDataPreparer, IMessanger messanger, ICommandTypesModel commandTypesModel, ISettings settings)
        {
            _command = command;
            _deviceFactory = deviceFactory;
            _message = message;
            _commandTranslator = commandTranslator;
            _incomingDataPreparer = incomingDataPreparer;
            _comMessanger = messanger;
            _commandTypesModel = commandTypesModel;
            _settings = settings;

            //_comcom.IncomingErrorReceived += OnErrorReceived;
        }

        public void Run()
        {
            //ISerialPortSettingsModel settings3 = new SerialPortSettingsModel();
            //settings3.BaudRate = 19200;//921600;//
            //settings3.DataBits = 8;
            //settings3.Parity = Parity.None;
            //settings3.PortNumber = 4;
            //settings3.StopBits = StopBits.One;
            //settings3.ReadTimeout = -1;
            //settings3.WriteTimeout = 100;
            //settings3.Encoding = Encoding.ASCII.CodePage;

            //_commandTypesModel = new CommandTypesModel();

            //_commandTypesModel.AddCommand("Revert");
            //_commandTypesModel.AddCommand("revert");
            //_commandTypesModel.AddCommand("Review");



            //IMoqPortSettingsModel settings2 = new MoqPortSettingsModel();
            //settings2.PortNumber = 6;

            //IApplicationSettingsModel appset = new ApplicationSettingsModel();
            //appset.DataSize = 30;
            //appset.IdSize = 4;
            //appset.CommandTypeSize = 4;
            //appset.Filler = '.';
            //appset.Separator = '*';

            //_settings.Add(SettingsType.CommandTypes, _commandTypesModel);
            //_settings.Add(SettingsType.SerialDevice, settings3);
            //_settings.Add(SettingsType.MoqDevice, settings2);
            ///_settings.Add(SettingsType.Application, appset);

            //_settings.SaveToFile();
            _settings.LoadFromFile(); 
            
            var aa = _settings.For(SettingsType.Application);
            Console.ReadLine();






            //IApplicationSettingsModel appset = new ApplicationSettingsModel();
            //appset.DataSize = 30;
            //appset.IdSize = 4;
            //appset.CommandTypeSize = 4;
            //appset.Filler = '.';
            //appset.Separator = '*';

            ////// testowanie zapisywania ustawien - działa -

            //ISerialPortSettingsModel settings = new SerialPortSettingsModel();
            //settings.BaudRate = 19200;//921600;//
            //settings.DataBits = 8;
            //settings.Parity = Parity.None;
            //settings.PortNumber = 4;
            //settings.StopBits = StopBits.One;
            //settings.ReadTimeout = -1;
            //settings.WriteTimeout = 100;
            //settings.Encoding = Encoding.ASCII.CodePage;

            //IMoqPortSettingsModel settings2 = new MoqPortSettingsModel();
            //settings2.PortNumber = 6;

            //AppSettings setman = new AppSettings(new AppSettingsModel());
            //_setman.Add(SettingsType.SerialDevice, settings);
            //_setman.Add(SettingsType.MoqDevice, settings2);
            //_setman.Add(SettingsType.Application, appset);

            //ICommandTypesModel commandTypesModel = new CommandTypesModel();
            //commandTypesModel.AddCommand("TestCommand");
            //_setman.Add(SettingsType.CommandTypes, commandTypesModel);






            //_setman.SaveToFile();

            ////foreach (var item in _setman.GetAll())
            ////{
            ////    Console.WriteLine("Pozycja sprzed wczytania:");
            ////    Console.WriteLine(item.Key.ToString() + " " + item.Value.ToString());
            ////}

            //// wczytywanie ustawien
            //_setman.LoadFromFile();

            //foreach(var c in _setman.GetSettingsFor<ICommandTypesModel>().CommandTypes)
            //{
            //    Console.WriteLine(c.Key.ToString());
            //}

           
            _comMessanger.Setup(DeviceType.Serial);

            //wyswietl porty w kompie
            foreach (var availablePort in SerialPort.GetPortNames())
            {
                Console.WriteLine(availablePort);
            }

            ////_comMessanger.Setup(DeviceType.Serial);

            //_comcom.AssignMessanger(_comMessanger);

            _comMessanger.Open();
            Task.Run(() => _comMessanger.StartReceiveAsync());
            Task.Run(() => _comMessanger.StartDecodingAsync());

            //int licznik = 0;

            //Console.WriteLine("Press enter to start sending test messages");

            //Console.ReadLine();

            ////_comMessanger.Setup(DeviceType.Serial);

            //// --- petla obiorczo nadawcza do testow!

            ////_comMessanger.Send(null);

            ////Task.Run(async () =>
            ////{
            ////    while (true)
            ////    {
            ////        CommandModel mes = new CommandModel();
            ////        mes.CommandType = CommandType.Data;
            ////        mes.Data = licznik.ToString();
            ////        mes.Id = Helpers.IdGenerator.GetID(((IApplicationSettingsModel)_setman.GetSettingsFor(SettingsType.Application)).IdSize);

            ////        await _comMessanger.SendAsync(mes);
            ////        licznik++;
            ////    }
            ////});

            //Console.WriteLine("ENTER to close communication line");
            //Console.ReadLine();
            //_comMessanger.StopDecoding();
            ////_comMessanger.Close();
            //Console.WriteLine("ENTER to open communication line");
            //Console.ReadLine();
            //Console.WriteLine("Com line opened.");
            //Task.Run(() => _comMessanger.StartDecodingAsync());
            //Console.WriteLine("ENTER to close communication line");
            //Console.ReadLine();
            //_comMessanger.Close();
            //Console.WriteLine("ENTER to open communication line");
            //Console.ReadLine();
            //Console.WriteLine("Com line opened.");
            //_comMessanger.Open();
            //Task.Run(() => _comMessanger.StartReceiveAsync());
            //Task.Run(() => _comMessanger.StartDecodingAsync());
            //Console.ReadLine();
            //Console.WriteLine("end after enter");
            //Console.ReadLine();
        }

        public void OnErrorReceived(object sender, MessageErrorEventArgs e)
        {
            Console.WriteLine("-------------------------------------------------------------------");
            Console.WriteLine(e.Message.Id);
            Console.WriteLine(e.Errors.ToString());
            foreach(var c in e.Message)
            {
                Console.WriteLine(c.CommandTypeString);
            }
        }
    }
}