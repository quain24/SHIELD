using Caliburn.Micro;
using Shield.Data;
using Shield.Enums;
using Shield.HardwareCom;
using Shield.HardwareCom.Factories;
using Shield.HardwareCom.Models;
using Shield.Data.Models;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace Shield.WpfGui.ViewModels
{
    // Data grid inside data grid.
    // Znalezc metode pobrania wybranego przez usera rzedu danych jako zrodla dla wewnetrznego data gridu

    public class ShellViewModel : Conductor<object>
    {
        private IMessanger _messanger;
        private ComCommander _comCommander;
        private IAppSettings _settings;
        private ICommandModelFactory _commandFactory;
        private string _selectedCommand;
        private string _dataInput;

        private BindableCollection<string> _possibleCommands = new BindableCollection<string>(Enum.GetNames(typeof(CommandType)));
        private BindableCollection<IMessageModel> _receivedMessages = new BindableCollection<IMessageModel>();
        private BindableCollection<ICommandModel> _newMessageCommands = new BindableCollection<ICommandModel>();
        private BindableCollection<IMessageModel> _sentMessages = new BindableCollection<IMessageModel>();
        private ICommandModel _selectedNewMessageCommand;
        private IMessageModel _selectedSentMessage;

        private IMessageModel _selectedReceivedMessage;

        private bool _receivingButtonActivated = false;

        public ShellViewModel(IMessanger messanger, IAppSettings settings, ICommandModelFactory commandFactory)
        {
            _settings = settings;
            _messanger = messanger;
            _commandFactory = commandFactory;
            _settings.LoadFromFile();

            _messanger.Setup(DeviceType.Serial);

            _comCommander = new ComCommander(_commandFactory, new Func<IMessageModel>(() => { return new MessageModel(); }));
            _comCommander.AssignMessanger(_messanger);

            _comCommander.IncomingMasterReceived += AddIncomingMessageToDisplay;
            _comCommander.IncomingSlaveReceived += AddIncomingMessageToDisplay;
            _comCommander.IncomingConfirmationReceived += AddIncomingMessageToDisplay;
            _comCommander.IncomingErrorReceived += AddIncomingMessageErrorToDisplay;
        }

        public int DataPackLength()
        {
            return _settings.GetSettingsFor<IApplicationSettingsModel>().DataSize;
        }

        public char DataPackFiller()
        {
            return _settings.GetSettingsFor<IApplicationSettingsModel>().Filler;
        }

        public List<string> DataPackGenerator(string data)
        {          
            int dataPackLength = DataPackLength();
            char filler = DataPackFiller();

            List<string> packs = Enumerable.Range(0, data.Length / DataPackLength())
                .Select(i => data.Substring(i * DataPackLength(), DataPackLength()))
                .ToList();

            int packsCumulativeLength = packs.Aggregate(0, (count, val) => count + val.Length);
            if(packsCumulativeLength < data.Length)
            {
                string lastOne = data.Substring(packsCumulativeLength);
                packs.Add(lastOne);
            }

            if(packs.Last().Length < dataPackLength)
                packs[packs.Count - 1] = packs.Last().PadRight(dataPackLength, filler);
            return packs;
        }

        public void AddIncomingMessageToDisplay(object sender, MessageEventArgs e)
        {
            ReceivedMessages.Add(e.Message);
        }

        public void AddIncomingMessageErrorToDisplay(object sender, MessageErrorEventArgs e)
        {
            ReceivedMessages.Add(e.Message);
        }

        public BindableCollection<IMessageModel> ReceivedMessages
        {
            get { return _receivedMessages; }
            set { _receivedMessages = value; }
        }

        public IMessageModel SelectedReceivedMessage
        {
            get { return _selectedReceivedMessage; }
            set
            {
                _selectedReceivedMessage = value;
                NotifyOfPropertyChange(() => SelectedReceivedMessage);
                NotifyOfPropertyChange(() => SingleMessageCommands);
            }
        }

        public BindableCollection<ICommandModel> SingleMessageCommands
        {
            get
            {
                return GetSingleMessageCommands();
            }
        }

        public BindableCollection<ICommandModel> GetSingleMessageCommands()
        {
            var output = new BindableCollection<ICommandModel>();

            if (_selectedReceivedMessage is null)
                return output;

            foreach (var c in SelectedReceivedMessage)
            {
                output.Add(c);
            }

            return output;
        }

        public bool CanOpenDevice
        {
            get
            {
                if (_messanger.IsOpen) return false;
                return true;
            }
        }

        public bool CanCloseDevice
        {
            get
            {
                if (_messanger is null) return false;
                if (_messanger.IsOpen) return true;
                return false;
            }
        }

        public void OpenDevice()
        {
            _messanger.Open();
            NotifyOfPropertyChange(() => CanOpenDevice);
            NotifyOfPropertyChange(() => CanCloseDevice);
            NotifyOfPropertyChange(() => CanStartReceiving);
            NotifyOfPropertyChange(() => CanStopReceiving);
            NotifyOfPropertyChange(() => ButtonAIsChecked);
            NotifyOfPropertyChange(() => CanStartReceiving);
        }

        public void CloseDevice()
        {
            _messanger.Close();
            NotifyOfPropertyChange(() => CanCloseDevice);
            NotifyOfPropertyChange(() => CanOpenDevice);
            NotifyOfPropertyChange(() => CanStartReceiving);
            NotifyOfPropertyChange(() => CanStopReceiving);
            NotifyOfPropertyChange(() => ButtonAIsChecked);
        }

        public bool CanStartReceiving
        {
            get
            {
                if (_receivingButtonActivated == false && _messanger.IsOpen) return true;
                return false;
            }
        }

        public bool ButtonAIsChecked
        {
            get
            {
                if (_messanger.IsOpen)
                {
                    return true;
                }
                else
                {
                    _receivingButtonActivated = false;
                    return false;
                }
            }
        }

        public void StartReceiving()
        {
            Task.Run(async () => await _messanger.StartReceiveAsync());
            Task.Run(async () => await _messanger.StartDecodingAsync());
            _receivingButtonActivated = true;
            NotifyOfPropertyChange(() => CanStartReceiving);
            NotifyOfPropertyChange(() => CanStopReceiving);
        }

        public bool CanStopReceiving
        {
            get
            {
                if (_receivingButtonActivated == true && _messanger.IsOpen) return true;
                return false;
            }
        }

        public void StopReceiving()
        {
            _messanger.StopDecoding();
            _messanger.StopReceiving();
            _receivingButtonActivated = false;
            NotifyOfPropertyChange(() => CanStartReceiving);
            NotifyOfPropertyChange(() => CanStopReceiving);
        }

        public BindableCollection<string> CommandTypes
        {
            set
            {
                _possibleCommands = value;
            }
            get
            {
                return _possibleCommands;
            }
        }

        public string SelectedCommand
        {
            get
            {
                return _selectedCommand;
            }
            set
            {
                _selectedCommand = value;
                NotifyOfPropertyChange(() => SelectedCommand);
                NotifyOfPropertyChange(() => DataInputState);
                NotifyOfPropertyChange(() => CanAddCommand);
            }
        }

        public bool DataInputState
        {
            get
            {
                if (_selectedCommand == Enum.GetName(typeof(CommandType), CommandType.Data))
                    return true;
                return false;
            }
        }

        public string DataInput
        {
            get
            {
                return _dataInput;
            }
            set
            {
                _dataInput = value;
                NotifyOfPropertyChange(() => DataInput);
                NotifyOfPropertyChange(() => CanAddCommand);
            }
        }

        public void AddCommand()
        {
            if(SelectedCommand is null)
                return;

            List<ICommandModel> commands = new List<ICommandModel>();

            if(SelectedCommand == Enum.GetName(typeof(CommandType), CommandType.Data))
            {
                List<string> packs = DataPackGenerator(DataInput);

                packs.ForEach(pack => 
                {
                    ICommandModel command = _commandFactory.Create(CommandType.Data);
                    command.Data = pack;                    
                    commands.Add(command);
                });

                NewMessageCommands.AddRange(commands);
            }
            else
                NewMessageCommands.Add(_commandFactory.Create((CommandType)Enum.Parse(typeof(CommandType), SelectedCommand)));
        }


        // TODO -> Add validation call when changing text, not just when leaving text box
        public bool CanAddCommand
        {
            get
            {
                if(SelectedCommand == Enum.GetName(typeof(CommandType), CommandType.Data))
                {
                    if(DataInput != null && DataInput.Length > 0 && !DataInput.Contains(DataPackFiller()) &&
                        !DataInput.Contains(_settings.GetSettingsFor<IApplicationSettingsModel>().Separator) &&
                        !DataInput.Contains(" "))
                    {
                        return true;
                    }
                    return false;
                }
                return true;
            }
        }

        public bool CanAddCommandEventHandler()
        {
            if( CanAddCommand)
            {
                AddCommand();
                return true;
            }
            return false;
        }

        public BindableCollection<ICommandModel> NewMessageCommands
        {
            get { return _newMessageCommands; }
            set { _newMessageCommands = value; }
        }

        public BindableCollection<IMessageModel> SentMessages
        {
            get { return _sentMessages; }
            set { _sentMessages = value; }
        }

        public ICommandModel SelectedNewMessageCommand
        {
            get => _selectedNewMessageCommand;
            
            set
            {
                _selectedNewMessageCommand = value;
                NotifyOfPropertyChange(() => SelectedNewMessageCommand);
            }
        }

        public IMessageModel SelectedSentMessage
        {
            get => _selectedSentMessage;
            
            set
            {
                _selectedSentMessage = value;
                NotifyOfPropertyChange(() => SelectedSentMessage);
            }
        }
    }
}