using Caliburn.Micro;
using Shield.Data;
using Shield.Enums;
using Shield.HardwareCom;
using Shield.HardwareCom.Factories;
using Shield.HardwareCom.Models;
using System;
using System.Threading.Tasks;

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

        private BindableCollection<IMessageModel> _receivedMessages = new BindableCollection<IMessageModel>();
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
    }
}