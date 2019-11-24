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
using System.Windows.Controls;
using System.Globalization;
using System.ComponentModel;
using Shield.WpfGui.Validators;
using System.Collections;
using Shield.Helpers;

namespace Shield.WpfGui.ViewModels
{
    // Data grid inside data grid.
    // Znalezc metode pobrania wybranego przez usera rzedu danych jako zrodla dla wewnetrznego data gridu

    public class ShellViewModel : Conductor<object>, INotifyDataErrorInfo
    {
        private IMessanger _messanger;
        private ComCommander _comCommander;
        private ISettings _settings;
        private ICommandModelFactory _commandFactory;
        private IMessageInfoAndErrorChecks _msgInfoError;
        private string _selectedCommand;
        private string _dataInput;


        

        private BindableCollection<string> _possibleCommands = new BindableCollection<string>(Enum.GetNames(typeof(CommandType)));
        private BindableCollection<IMessageModel> _receivedMessages = new BindableCollection<IMessageModel>();
        private BindableCollection<ICommandModel> _newMessageCommands = new BindableCollection<ICommandModel>();
        private BindableCollection<IMessageModel> _sentMessages = new BindableCollection<IMessageModel>();
        private ICommandModel _selectedNewMessageCommand;
        private IMessageModel _selectedSentMessage;

        private Func<IMessageModel> _messageFactory;

        private IMessageModel _selectedReceivedMessage;

        private bool _receivingButtonActivated = false;
        private bool _sending = false;


        private CommandDataPackValidation _dataPackValidation;
        private readonly Dictionary<string, ICollection<string>>
        _validationErrors = new Dictionary<string, ICollection<string>>();
        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        public ShellViewModel(IMessanger messanger, ISettings settings, ICommandModelFactory commandFactory, IMessageInfoAndErrorChecks msgInfoError)
        {
            _settings = settings;
            _messanger = messanger;
            _commandFactory = commandFactory;
            _settings.LoadFromFile();

            //_settings.GetSettingsFor<ISerialPortSettingsModel>().BaudRate = 19200;
            //_settings.SaveToFile();

            _messanger.Setup(DeviceType.Serial);
            _messageFactory = new Func<IMessageModel>(() => { return new MessageModel(); });

            _msgInfoError = msgInfoError;

            _comCommander = new ComCommander(_commandFactory, _messageFactory, _msgInfoError);
            _comCommander.AssignMessanger(_messanger);

            _comCommander.IncomingMasterReceived += AddIncomingMessageToDisplay;
            _comCommander.IncomingSlaveReceived += AddIncomingMessageToDisplay;
            _comCommander.IncomingConfirmationReceived += AddIncomingMessageToDisplay;
            _comCommander.IncomingErrorReceived += AddIncomingMessageErrorToDisplay;
            
            _dataPackValidation = new CommandDataPackValidation(_settings.ForTypeOf<IApplicationSettingsModel>().Separator, DataPackFiller());
        }

        public int DataPackLength()
        {
            return _settings.ForTypeOf<IApplicationSettingsModel>().DataSize;
        }

        public char DataPackFiller()
        {
            return _settings.ForTypeOf<IApplicationSettingsModel>().Filler;
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
            NotifyOfPropertyChange(() => CanSendMessage);
        }

        public void CloseDevice()
        {
            _messanger.Close();
            NotifyOfPropertyChange(() => CanCloseDevice);
            NotifyOfPropertyChange(() => CanOpenDevice);
            NotifyOfPropertyChange(() => CanStartReceiving);
            NotifyOfPropertyChange(() => CanStopReceiving);
            NotifyOfPropertyChange(() => ButtonAIsChecked);
            NotifyOfPropertyChange(() => CanSendMessage);
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
                ValidateCommandDataPack(_dataInput);
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
            NotifyOfPropertyChange(()=> CanSendMessage);
            NotifyOfPropertyChange(() => NewMessageCommands);
        }

        public bool CanAddCommand
        {
            get
            {
                if(SelectedCommand == Enum.GetName(typeof(CommandType), CommandType.Data))
                {
                    if(_validationErrors.ContainsKey("DataInput") && _validationErrors["DataInput"].Count > 0)
                        return false;

                    if(DataInput != null && DataInput.Length > 0 && !DataInput.Contains(DataPackFiller()) &&
                        !DataInput.Contains(_settings.ForTypeOf<IApplicationSettingsModel>().Separator) &&
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
            set 
            {
                _newMessageCommands = value;
                NotifyOfPropertyChange(() => CanRemoveCommand);
                NotifyOfPropertyChange(() => NewMessageCommands);
            }
        }

        public BindableCollection<IMessageModel> SentMessages
        {
            get { return _sentMessages; }
            set
            {
                _sentMessages = value;
                // hack!
                _sending = false;
            }
        }

        public ICommandModel SelectedNewMessageCommand
        {
            get => _selectedNewMessageCommand;
            
            set
            {
                _selectedNewMessageCommand = value;
                NotifyOfPropertyChange(() => SelectedNewMessageCommand);
                NotifyOfPropertyChange(() => CanRemoveCommand);
            }
        }

        public IMessageModel SelectedSentMessage
        {
            get => _selectedSentMessage;
            
            set
            {
                _selectedSentMessage = value;
                NotifyOfPropertyChange(() => SelectedSentMessage);
                NotifyOfPropertyChange(() => SingleSelectedSentMessage);
            }
        }

        public BindableCollection<ICommandModel> SingleSelectedSentMessage
        {
            get
            {
                var output = new BindableCollection<ICommandModel>();

                if(SelectedSentMessage is null)
                    return output;
                
                foreach(var c in SelectedSentMessage)
                {
                    output.Add(c);
                }
                return output;
            }
        }


        public void RemoveCommand()
        {
            NewMessageCommands.Remove(SelectedNewMessageCommand);
            NotifyOfPropertyChange(()=> CanSendMessage);
        }

        public bool CanRemoveCommand
        {
            get
            {
                if(SelectedNewMessageCommand is null)
                    return false;
                return true;
            }
        }

        public async Task SendMessage()
        {
            var message = GenerateMessage(NewMessageCommands);
            if(message is null)
                return;
            _comCommander.AddToSendingQueue(message);

            // hack!
            _sending = true;
            NotifyOfPropertyChange(()=> CanSendMessage);
            // hack end
            bool sent = await _comCommander.SendQueuedMessages(new System.Threading.CancellationToken());

            // hack
            _sending = false;
            if (sent)
            {
                SentMessages.Add(message);
                NewMessageCommands.Clear();
                NotifyOfPropertyChange(()=> NewMessageCommands);
                NotifyOfPropertyChange(()=> SentMessages);
                NotifyOfPropertyChange(()=> CanSendMessage);
            }
        }

        public bool CanSendMessage
        {
            get
            {
                if(NewMessageCommands.Count < 1 || _comCommander.DeviceIsOpen == false || _sending == true)
                {
                    return false;
                }
                return true;
            }
        }

        private IMessageModel GenerateMessage(IEnumerable<ICommandModel> commands)
        {
            if(commands.Count() == 0 || commands is null)
                return null;

            IMessageModel message = _messageFactory();

            foreach(var c in commands)
            {
                message.Add(c);
            }

            message.Timestamp = Timestamp.TimestampNow;
            message.AssaignID(IdGenerator.GetID(_settings.ForTypeOf<IApplicationSettingsModel>().IdSize));

            return message;
        }

        public bool HasErrors
        {
            get { return _validationErrors.Count > 0; }
        }

        private async void ValidateCommandDataPack(string data)
        {
            const string propertyKey = "DataInput";
            ICollection<string> validationErrors = null;
            /* Call service asynchronously */
            bool isValid = await Task<bool>.Run(() => 
            { 
                return _dataPackValidation.ValidateDataPack(data, out validationErrors); 
            })
            .ConfigureAwait(false);
 
            if (!isValid)
            {
                /* Update the collection in the dictionary returned by the GetErrors method */
                _validationErrors[propertyKey] = validationErrors;
                /* Raise event to tell WPF to execute the GetErrors method */
                RaiseErrorsChanged(propertyKey);
            }
            else if(_validationErrors.ContainsKey(propertyKey))
            {
                /* Remove all errors for this property */
                _validationErrors.Remove(propertyKey);
                /* Raise event to tell WPF to execute the GetErrors method */
                RaiseErrorsChanged(propertyKey);
            }
        }

        public IEnumerable GetErrors(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName)
                || !_validationErrors.ContainsKey(propertyName))
                return null;
 
            return _validationErrors[propertyName];
        }

        private void RaiseErrorsChanged(string propertyName)
        {
            if (ErrorsChanged != null)
                ErrorsChanged(this, new DataErrorsChangedEventArgs(propertyName));
        }
        
    }       
}