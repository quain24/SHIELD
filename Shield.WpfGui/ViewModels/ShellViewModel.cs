using Caliburn.Micro;
using Shield.CommonInterfaces;
using Shield.Data;
using Shield.Data.Models;
using Shield.Enums;
using Shield.HardwareCom;
using Shield.HardwareCom.Factories;
using Shield.HardwareCom.MessageProcessing;
using Shield.HardwareCom.Models;
using Shield.Helpers;
using Shield.WpfGui.Validators;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Ports;
using System.Linq;
using System.Threading.Tasks;

namespace Shield.WpfGui.ViewModels
{
    public class ShellViewModel : Conductor<object>, INotifyDataErrorInfo
    {
        private HardwareCom.IMessenger _messanger;
        private ISettings _settings;
        private ICommandModelFactory _commandFactory;
        private ICommandIngester _commandIngester;
        private IIncomingMessageProcessor _incomingMessageProcessor;
        private IConfirmationFactory _confirmationFactory;
        private IConfirmationTimeoutChecker _confirmationTimeoutChecker;
        private readonly IIdGenerator _idGenerator;
        private readonly MessengingPipeline _incomingMessagePipeline;
        private readonly ISerialPortSettingsContainer _serialPortSettingsContainer;
        private readonly ICommunicationDeviceFactory _communicationDeviceFactory;
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
        private bool _openingError = false;

        private CommandDataPackValidation _dataPackValidation;

        private readonly Dictionary<string, ICollection<string>>
        _validationErrors = new Dictionary<string, ICollection<string>>();

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        public ShellViewModel(HardwareCom.IMessenger messanger,
                              ISettings settings,
                              ICommandModelFactory commandFactory,
                              Func<IMessageModel> messageFactory,
                              ICommandIngester commandIngester,
                              IIncomingMessageProcessor incomingMessageProcessor,
                              IConfirmationFactory confirmationFactory,
                              IConfirmationTimeoutChecker confirmationTimeoutChecker,
                              IIdGenerator idGenerator,
                              MessengingPipeline incomingMessagePipeline,
                              ISerialPortSettingsContainer serialPortSettingsContainer,
                              ICommunicationDeviceFactory communicationDeviceFactory)
        {
            _settings = settings;
            _messanger = messanger;
            _commandFactory = commandFactory;
            _commandIngester = commandIngester;
            _incomingMessageProcessor = incomingMessageProcessor;
            _confirmationFactory = confirmationFactory;
            _confirmationTimeoutChecker = confirmationTimeoutChecker;
            _idGenerator = idGenerator;
            _incomingMessagePipeline = incomingMessagePipeline;
            _serialPortSettingsContainer = serialPortSettingsContainer;
            _communicationDeviceFactory = communicationDeviceFactory;
            _messageFactory = messageFactory;

            _settings.LoadFromFile();
            _settings.SaveToFile();

            //serial.Setup(port_set);
            
            //_serialPortSettingsContainer.Add(portset);
            //_settings.ForTypeOf<ISerialPortSettingsContainer>().GetSettingsByPortNumber(5);


            //_settings.AddOrReplace(SettingsType.SerialDeviceContainer, _serialPortSettingsContainer);
            //_settings.SaveToFile();

            _dataPackValidation = new CommandDataPackValidation(_settings.ForTypeOf<IApplicationSettingsModel>().Separator, DataPackFiller());

            _incomingMessageProcessor.SwitchSourceCollection(_commandIngester.GetReceivedMessages());

            //if(_confirmationTimeoutChecker.Timeout != _confirmationTimeoutChecker.NoTimeoutValue)
            Task.Run(async () => await _confirmationTimeoutChecker.CheckUnconfirmedMessagesContinousAsync().ConfigureAwait(false));

            // Updating table in gui
            Task.Run(async () =>
            {
                while (true)
                {
                    IMessageModel message = _incomingMessagePipeline.GetReceivedMessages().Take();//_incomingMessageProcessor.GetProcessedMessages().Take();
                    AddIncomingMessageToDisplay(this, message);
                    //if (message.Type != MessageType.Confirmation)
                    //{
                    //    IMessageModel confirmation = _confirmationFactory.GenetateConfirmationOf(message);
                    //    await _messanger.SendAsync(confirmation).ConfigureAwait(false);
                    //    SentMessages.Add(confirmation);
                    //}
                    //else
                    //{
                    //    _confirmationTimeoutChecker.AddConfirmation(message);
                    //}
                }
            });

            //_messanger.CommandReceived += AddCommandToProcessing;
        }

        // new tryouts

        public void AddCommandToProcessing(object sender, ICommandModel e)
        {
            _commandIngester.AddCommandToProcess(e);
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

        public void AddIncomingMessageToDisplay(object sender, IMessageModel e)
        {
            ReceivedMessages.Add(e);
        }

        public void AddIncomingMessageErrorToDisplay(object sender, IMessageModel e)
        {
            ReceivedMessages.Add(e);
        }

        public BindableCollection<IMessageModel> ReceivedMessages
        {
            get => _receivedMessages;
            set => _receivedMessages = value;
        }

        public IMessageModel SelectedReceivedMessage
        {
            get => _selectedReceivedMessage;
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
                if (_openingError)
                {
                    _openingError = false;
                    return true;
                }
                return false;
            }
        }

        public void OpenDevice()
        {
            try
            {
                //_messanger.Open();
                _incomingMessagePipeline.Start();
                NotifyOfPropertyChange(() => CanOpenDevice);
                NotifyOfPropertyChange(() => CanCloseDevice);
                NotifyOfPropertyChange(() => CanStartReceiving);
                NotifyOfPropertyChange(() => CanStopReceiving);
                NotifyOfPropertyChange(() => ButtonAIsChecked);
                NotifyOfPropertyChange(() => CanStartReceiving);
                NotifyOfPropertyChange(() => CanSendMessage);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);

                // Enables Close port button in case of exception, enables CanOpenDevice to return TRUE
                _openingError = true;
                NotifyOfPropertyChange(() => CanOpenDevice);
                NotifyOfPropertyChange(() => CanCloseDevice);
            }
        }

        public void CloseDevice()
        {
            //_messanger.Close();
            _incomingMessagePipeline.Stop();
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

        public bool ButtonAIsChecked => _messanger.IsOpen ? true : _receivingButtonActivated = false;

        public void StartReceiving()
        {
            Task.Run(() => _messanger.StartReceiveingAsync());
            //Task.Run(() => _messanger.StartDecoding());

            Task.Run(() => _commandIngester.StartProcessingCommands()).ConfigureAwait(false);
            Task.Run(() => _commandIngester.StartProcessingCommands()).ConfigureAwait(false);
            Task.Run(() => _commandIngester.StartProcessingCommands()).ConfigureAwait(false);
            Task.Run(() => _commandIngester.StartTimeoutCheckAsync().ConfigureAwait(false)).ConfigureAwait(false);
            Task.Run(() => _incomingMessageProcessor.StartProcessingMessagesContinous()).ConfigureAwait(false);

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
            _incomingMessageProcessor.StopProcessingMessages();
            _commandIngester.StopTimeoutCheck();
            _commandIngester.StopProcessingCommands();

            _messanger.StopReceiving();
            _receivingButtonActivated = false;
            NotifyOfPropertyChange(() => CanStartReceiving);
            NotifyOfPropertyChange(() => CanStopReceiving);
        }

        public BindableCollection<string> CommandTypes
        {
            set => _possibleCommands = value;
            get => _possibleCommands;
        }

        public string SelectedCommand
        {
            get => _selectedCommand;
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
                if (_selectedCommand == GetNameFromEnumValue(CommandType.Data))
                    return true;
                return false;
            }
        }

        private string GetNameFromEnumValue(CommandType value)
        {
            return Enum.GetName(value.GetType(), value);
        }

        public string DataInput
        {
            get => _dataInput;
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

            var commands = new List<ICommandModel>();

            if (SelectedCommand == GetNameFromEnumValue(CommandType.Data))
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
                if (SelectedCommand == GetNameFromEnumValue(CommandType.Data))
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
            get => _newMessageCommands;
            set
            {
                _newMessageCommands = value;
                NotifyOfPropertyChange(() => CanRemoveCommand);
                NotifyOfPropertyChange(() => NewMessageCommands);
            }
        }

        public BindableCollection<IMessageModel> SentMessages
        {
            get => _sentMessages;
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

                if (SelectedSentMessage is null)
                    return output;

                foreach (var c in SelectedSentMessage)
                    output.Add(c);

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
            get => SelectedNewMessageCommand is null ? false : true;
        }

        public async Task SendMessage()
        {
            var message = GenerateMessage(NewMessageCommands);
            if (message is null)
                return;

            // hack!
            _sending = true;
            NotifyOfPropertyChange(() => CanSendMessage);

            bool sent = await _messanger.SendAsync(message).ConfigureAwait(false);

            // hack
            _sending = false;
            if (sent)
            {
                SentMessages.Add(message);
                _confirmationTimeoutChecker.AddToCheckingQueue(message);

                NewMessageCommands.Clear();
                NotifyOfPropertyChange(() => NewMessageCommands);
                NotifyOfPropertyChange(() => SentMessages);
                NotifyOfPropertyChange(() => CanSendMessage);
            }
        }

        public bool CanSendMessage
        {
            get
            {
                if (NewMessageCommands.Count < 1 || _messanger.IsOpen == false || _sending == true)
                    return false;

                return true;
            }
        }

        private IMessageModel GenerateMessage(IEnumerable<ICommandModel> commands)
        {
            if (commands.Count() == 0 || commands is null)
                return null;

            IMessageModel message = _messageFactory();

            foreach (var c in commands)
            {
                message.Add(c);
            }

            message.Timestamp = Timestamp.TimestampNow;
            message.AssaignID(_idGenerator.GetNewID());

            return message;
        }

        public bool HasErrors
        {
            get => _validationErrors.Count > 0;
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