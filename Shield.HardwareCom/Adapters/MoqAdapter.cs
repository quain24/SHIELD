﻿using Shield.CommonInterfaces;
using Shield.HardwareCom.Models;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Shield.HardwareCom.Adapters
{
    public class MoqAdapter : ICommunicationDevice
    {
        public event EventHandler<string> DataReceived;

        private string _portName;
        private string _rawData;
        private bool _isOpen = false;

        public bool IsOpen { get; }
        public int ConfirmationTimeout { get; set; }
        public int CompletitionTimeout { get; set; }
        public string Name { get; private set; }

        public MoqAdapter(string portName)
        {
            _portName = portName;
            //.DataReceived += PropagateDataReceivedEvent;
        }

        public async Task CloseAsync()
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
            CloseAsync();
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
            return new CommandModel { CommandType = Enums.CommandType.Data, Data = _portName + " " + _rawData };
        }

        public async Task<bool> SendAsync(string command)
        {
            Debug.WriteLine("ASYNC Wysłano dane do portwu obiektu \"MoqAdapter\"");
            Debug.WriteLine("ASYNC Odpalono event DataReceived \"MoqAdapter\"");
            _rawData = command;
            DataReceived?.Invoke(this, "");
            return true;
        }

        public bool Send(string command)
        {
            Debug.WriteLine("Wysłano dane do portwu obiektu \"MoqAdapter\"");
            Debug.WriteLine("Odpalono event DataReceived \"MoqAdapter\"");
            _rawData = command;
            DataReceived?.Invoke(this, "");
            return true;
        }

        private void PropagateDataReceivedEvent(object sender, string e)
        {
            DataReceived?.Invoke(sender, e);
            Debug.WriteLine("Odpalono event PropagateDataReceivedEvent z \"MoqAdapter\"");
        }

        public bool Setup(ICommunicationDeviceSettings settings)
        {
            IMoqPortSettingsModel internalSettings = (IMoqPortSettingsModel)settings;

            _portName = internalSettings.PortNumber.ToString();
            CompletitionTimeout = internalSettings.CompletitionTimeout;
            ConfirmationTimeout = internalSettings.ConfirmationTimeout;
            Name = internalSettings.Name;

            return true;
        }

        public void StartReceiving()
        {
            throw new NotImplementedException();
        }

        public Task StartReceivingAsync()
        {
            throw new NotImplementedException();
        }

        //async Task ICommunicationDevice.StartReceivingAsync()
        //{
        //    throw new NotImplementedException();
        //}

        public void StopReceiving()
        {
            throw new NotImplementedException();
        }

        public void StopSending()
        {
            throw new NotImplementedException();
        }

        Task<string> ICommunicationDevice.ReceiveAsync(CancellationToken cancellToken)
        {
            throw new NotImplementedException();
        }

        public Task<string> StartReceivingAsync(CancellationToken cancellToken)
        {
            throw new NotImplementedException();
        }

        public Task<bool> SendAsync(string command, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public void Close()
        {
            throw new NotImplementedException();
        }
    }
}