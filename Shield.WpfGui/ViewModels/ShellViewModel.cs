using Caliburn.Micro;
using Shield.HardwareCom.Adapters;
using System;
using System.Diagnostics;
using Shield;
using Shield.Enums;
using Shield.HardwareCom;
using Shield.Data;
using Shield.Helpers;
using Shield.Extensions;
using Shield.CommonInterfaces;
using Shield.HardwareCom.Factories;
using Shield.Data.Models;
using Autofac.Features.Indexed;
using Shield.HardwareCom.Models;
using System.Threading.Tasks;

namespace Shield.WpfGui.ViewModels
{
    public class ShellViewModel : Conductor<object>
    {
        IMessanger _messanger;
        ComCommander _comCommander;
        ICommunicationDeviceFactory _comDevFac;
        IAppSettings _settings;
        ICommandModelFactory _commandFactory;
        
        private BindableCollection<IMessageModel> _receivedMessages = new BindableCollection<IMessageModel>();
        private IMessageModel _selectedMessage;

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

        public IMessageModel SelectedReceivedMessages
        {
            get { return _selectedMessage; }
            set
            { 
                _selectedMessage = value;
                NotifyOfPropertyChange(() => SelectedReceivedMessages);
            }
        }



        public bool CanOpenDevice
        {
            get
            {
                if(_messanger.IsOpen) return false;
                return true;
            }
        }        

        public void OpenDevice()
        {
            _messanger.Open();
            NotifyOfPropertyChange(() => CanOpenDevice);
            NotifyOfPropertyChange(() => CanStartReceiving);
        }

        public bool CanStartReceiving
        {
            get
            {
                if(_messanger.IsOpen) return true;
                return false;
            }
        }

        public void StartReceiving()
        {
            Task.Run(async () => await _messanger.StartReceiveAsync());
            Task.Run(async () => await _messanger.StartDecodingAsync());
            NotifyOfPropertyChange(() => CanStartReceiving);
        }
        
    }
}