using Caliburn.Micro;
using Shield.Data;
using Shield.Data.Models;
using Shield.Enums;
using Shield.HardwareCom;
using Shield.HardwareCom.Factories;
using Shield.HardwareCom.Models;
using Shield.Helpers;
using Shield.WpfGui.Validators;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace Shield.WpfGui.ViewModels
{
    // Data grid inside data grid.
    // Znalezc metode pobrania wybranego przez usera rzedu danych jako zrodla dla wewnetrznego data gridu

    public class ShellViewModel : Conductor<object>, INotifyDataErrorInfo
    {
        private IMessanger _messanger;
        private ISettings _settings;
        private ICommandModelFactory _commandFactory;
        private ICommandIngester _commandIngester;
        private IMessageProcessor _incomingMessageProcessor;


        private string _selectedCommand;
        private string _dataInput;

        private BindableCollection<string> _possibleCommands = new BindableCollection<string>(Enum.GetNames(typeof(CommandType)));
        private BindableCollection<IMessageHWComModel> _receivedMessages = new BindableCollection<IMessageHWComModel>();
        private BindableCollection<ICommandModel> _newMessageCommands = new BindableCollection<ICommandModel>();
        private BindableCollection<IMessageHWComModel> _sentMessages = new BindableCollection<IMessageHWComModel>();
        private ICommandModel _selectedNewMessageCommand;
        private IMessageHWComModel _selectedSentMessage;

        private Func<IMessageHWComModel> _messageFactory;

        private IMessageHWComModel _selectedReceivedMessage;

        private bool _receivingButtonActivated = false;
        private bool _sending = false;

        private CommandDataPackValidation _dataPackValidation;

        private readonly Dictionary<string, ICollection<string>>
        _validationErrors = new Dictionary<string, ICollection<string>>();

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        public ShellViewModel(IMessanger messanger,
                              ISettings settings,
                              ICommandModelFactory commandFactory,
                              ICommandIngester commandIngester,
                              IMessageProcessor incomingMessageProcessor)
        {
            _settings = settings;
            _messanger = messanger;
            _commandFactory = commandFactory;
            _commandIngester = commandIngester;
            _incomingMessageProcessor = incomingMessageProcessor;

            _settings.LoadFromFile();
            _messanger.Setup(DeviceType.Serial);
            _dataPackValidation = new CommandDataPackValidation(_settings.ForTypeOf<IApplicationSettingsModel>().Separator, DataPackFiller());

            //Task.Run
            Task.Run(() => _incomingMessageProcessor.StartProcessingMessagesContinous()).ConfigureAwait(false);
            
            Task.Run(() => _commandIngester.StartProcessingCommands()).ConfigureAwait(false);

            



            

            _messanger.CommandReceived += AddCommandToProcessing;
            
        }

        // new tryouts

        public void AddCommandToProcessing(object sender, ICommandModel e)
        {
            
            
            Task.Run(() => _commandIngester.AddCommandToProcess(e));
            Task.Run(() =>
            {

            
            foreach(var c in _commandIngester.GetProcessedMessages().GetConsumingEnumerable())
            {
                _incomingMessageProcessor.AddMessageToProcess(c);
            }  
            });

            Task.Run(() =>
            {
                foreach(var m in _incomingMessageProcessor.GetProcessedMessages().GetConsumingEnumerable())
                {
                    AddIncomingMessageToDisplay(this, m);
                }
            });
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
            if (packsCumulativeLength < data.Length)
            {
                string lastOne = data.Substring(packsCumulativeLength);
                packs.Add(lastOne);
            }

            if (packs.Last().Length < dataPackLength)
                packs[packs.Count - 1] = packs.Last().PadRight(dataPackLength, filler);
            return packs;
        }

        public void AddIncomingMessageToDisplay(object sender, IMessageHWComModel e)
        {
            ReceivedMessages.Add(e);
        }

        public void AddIncomingMessageErrorToDisplay(object sender, IMessageHWComModel e)
        {
            ReceivedMessages.Add(e);
        }

        public BindableCollection<IMessageHWComModel> ReceivedMessages
        {
            get { return _receivedMessages; }
            set { _receivedMessages = value; }
        }

        public IMessageHWComModel SelectedReceivedMessage
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
            Task.Run(/*async*/ () => /*await*/ _messanger.StartReceiveAsync());
            Task.Run(/*async*/ () => /*await*/ _messanger.StartDecodingAsync());
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
            if (SelectedCommand is null)
                return;

            List<ICommandModel> commands = new List<ICommandModel>();

            if (SelectedCommand == Enum.GetName(typeof(CommandType), CommandType.Data))
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
            NotifyOfPropertyChange(() => CanSendMessage);
            NotifyOfPropertyChange(() => NewMessageCommands);
        }

        public bool CanAddCommand
        {
            get
            {
                if (SelectedCommand == Enum.GetName(typeof(CommandType), CommandType.Data))
                {
                    if (_validationErrors.ContainsKey("DataInput") && _validationErrors["DataInput"].Count > 0)
                        return false;

                    if (DataInput != null && DataInput.Length > 0 && !DataInput.Contains(DataPackFiller()) &&
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
            if (CanAddCommand)
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

        public BindableCollection<IMessageHWComModel> SentMessages
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

        public IMessageHWComModel SelectedSentMessage
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

                if (SelectedSentMessage is null)
                    return output;

                foreach (var c in SelectedSentMessage)
                {
                    output.Add(c);
                }
                return output;
            }
        }

        public void RemoveCommand()
        {
            NewMessageCommands.Remove(SelectedNewMessageCommand);
            NotifyOfPropertyChange(() => CanSendMessage);
        }

        public bool CanRemoveCommand
        {
            get
            {
                if (SelectedNewMessageCommand is null)
                    return false;
                return true;
            }
        }

        public async Task SendMessage()
        {
            var message = GenerateMessage(NewMessageCommands);
            if (message is null)
                return;
            //_comCommander.AddToSendingQueue(message);

            // hack!
            _sending = true;
            NotifyOfPropertyChange(() => CanSendMessage);
            // hack end
            
            //bool sent = await _comCommander.SendQueuedMessages(new System.Threading.CancellationToken());

            // hack
            _sending = false;
            //if (sent)
            //{
            //    SentMessages.Add(message);
            //    NewMessageCommands.Clear();
            //    NotifyOfPropertyChange(() => NewMessageCommands);
            //    NotifyOfPropertyChange(() => SentMessages);
            //    NotifyOfPropertyChange(() => CanSendMessage);
            //}
        }

        public bool CanSendMessage
        {
            get
            {
                if (NewMessageCommands.Count < 1 || _messanger.IsOpen == false || _sending == true)
                {
                    return false;
                }
                return true;
            }
        }

        private IMessageHWComModel GenerateMessage(IEnumerable<ICommandModel> commands)
        {
            if (commands.Count() == 0 || commands is null)
                return null;

            IMessageHWComModel message = _messageFactory();

            foreach (var c in commands)
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
            else if (_validationErrors.ContainsKey(propertyKey))
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