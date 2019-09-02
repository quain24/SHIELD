﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shield.CommonInterfaces;
using Shield.HardwareCom.Models;

namespace Shield.HardwareCom.Adapters
{
    public class SerialPortAdapter : ICommunicationDevice
    {
        /// <summary>
        /// Wraps SerialPort for future use with interfaces
        /// </summary>
        private readonly SerialPort _port;
        private readonly int _commandSize;
        private string _receivedBuffer = string.Empty;

        public event EventHandler DataReceived;
        
        public SerialPortAdapter(SerialPort port, int commandSize)
        {
            _port = port;
            _commandSize = commandSize;
            // zmienic na lokalna metode, ktora utworzy kompletny message i dopiero podniesie event na zewnatrz?
            _port.DataReceived += PropagateDataReceivedEvent;
        }             

        private void PropagateDataReceivedEvent(object sender, EventArgs e)
        {
            this.DataReceived?.Invoke(sender, e);
        }        
        
        public void Open()
        {
            if (_port != null && !_port.IsOpen)
            {
                _port.Open();
            }
        }
        public void Close()
        {
            // Close the serial port in a new thread
            Task closeTask = new Task(() =>
            {
                try
                {
                    _port.Close();
                }
                catch (IOException e)
                {
                    // Port was not open
                    Debug.WriteLine("MESSAGE: SerialPortAdapter Close - Port was not open! " + e.Message);
                }
            });
            closeTask.Start();

            //return closeTask;

            // odniorca:
            // await serialStream.Close();
        }

        public void DiscardInBuffer()
        {
            _port.DiscardInBuffer();
            _receivedBuffer = string.Empty;
        }        

        public ICommandModel Receive()
        {   
            _receivedBuffer += _port.ReadExisting();






            return new CommandModel {CommandType = Enums.CommandType.Data, Data = "Test data readed from SerialPortAdapter" };  // do testow, imoplementacja czeka!
        }

        public void Send(ICommandModel command)
        {
            // opracować co jeżeli nic nie zostanie zapisane - handle exceptions!
            // przerobienie na typ string, wybór co ma wysłać i jak - tutaj czy w messanger?
            _port.Write(command.CommandTypeString);
        }

        

        public void Dispose()
        {   
            _port.DataReceived -= PropagateDataReceivedEvent;
            _port.Close();
        }
    }
}
