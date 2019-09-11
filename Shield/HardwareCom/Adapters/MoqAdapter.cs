using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shield.CommonInterfaces;
using System.IO.Ports;
using Shield.HardwareCom.Models;
using Shield.Data.Models;

namespace Shield.HardwareCom.Adapters
{
    public class MoqAdapter : ICommunicationDevice
    {
        public event EventHandler<ICommandModel> DataReceived;
        private string _portName;
        private string _rawData;
        private bool _isOpen = false;

        public MoqAdapter(string portName)
        {
            _portName = portName;
            //.DataReceived += PropagateDataReceivedEvent;
        }

        public void Close()
        {
            Debug.WriteLine("Zamknięto obiekt \"MoqAdapter\"");
            _isOpen = false;
        }

        public void DiscardInBuffer()
        {
            Debug.WriteLine("Wyczyszcono bufor obiektu \"MoqAdapter\"");
            _rawData = string.Empty;
        }

        public void Dispose()
        {
            Close();
            Debug.WriteLine("Zamknięto / Dispose obiekt \"MoqAdapter\"");
            //_port.DataReceived -= PropagateDataReceivedEvent;
        }

        public void Open()
        {
            Debug.WriteLine("Otwarto port w obiekcie \"MoqAdapter\"");
            _isOpen = true;
        }

        public ICommandModel Receive()
        {
            Debug.WriteLine("Odczytano dane z obiektu - wybrany port plus dane wprowadzone  \"MoqAdapter\"");
            return  new CommandModel {CommandType = Enums.CommandType.Data, Data =  _portName + " " + _rawData };            
        }

        public async Task<bool> SendAsync(ICommandModel command)
        {
            Debug.WriteLine("ASYNC Wysłano dane do portwu obiektu \"MoqAdapter\"");
            Debug.WriteLine("ASYNC Odpalono event DataReceived \"MoqAdapter\"");
            _rawData = command.CommandTypeString;
            DataReceived?.Invoke(this, new CommandModel());
            return true;
        }

        public bool Send(ICommandModel command)
        {            
            Debug.WriteLine("Wysłano dane do portwu obiektu \"MoqAdapter\"");
            Debug.WriteLine("Odpalono event DataReceived \"MoqAdapter\"");
            _rawData = command.CommandTypeString;
            DataReceived?.Invoke(this, new CommandModel());
            return true;
        }




        private void PropagateDataReceivedEvent(object sender, ICommandModel e)
        {
            DataReceived?.Invoke(sender, e);
            Debug.WriteLine("Odpalono event PropagateDataReceivedEvent z \"MoqAdapter\"");
        }

        public bool Setup(ICommunicationDeviceSettings settings)
        {
            IMoqPortSettingsModel internalSettings = (IMoqPortSettingsModel) settings;

            _portName = internalSettings.PortNumber.ToString();

            return true;
        }

        public void StartReceiving()
        {
            throw new NotImplementedException();
        }

        Task ICommunicationDevice.StartReceiving()
        {
            throw new NotImplementedException();
        }

        public Task StartReceivingAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<int> ReadUsingStream()
        {
            throw new NotImplementedException();
        }
    }
}
