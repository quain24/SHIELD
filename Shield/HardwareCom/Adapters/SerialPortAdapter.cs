using Shield.CommonInterfaces;
using Shield.Data;
using Shield.Data.Models;
using Shield.Enums;
using Shield.Extensions;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Shield.HardwareCom.Adapters
{
    // Przeanalizować tą klasę, popoprawiać i zoptymalizować co jest, upewnić się, że nie ma potrzeby niczego dodatkowego i zakończyć!

    public class SerialPortAdapter : ICommunicationDevice
    {
        /// <summary>
        /// Wraps SerialPort for future use with interfaces
        /// Works on commandMNodel's, sends and receives them with translation from and into them
        /// </summary>

        #region Internal variables and events

        private CancellationTokenSource _receiveTokenSource = new CancellationTokenSource();
        private CancellationToken _receiveToken;
        private CancellationTokenSource _sendTokenSource = new CancellationTokenSource();
        private CancellationToken _sendToken;

        private object _lock = new object();

        private readonly SerialPort _port;
        
        private bool _isLissening = false;
        private ISerialPortSettingsModel _internalSettings;

        public event EventHandler<string> DataReceived;

        #endregion Internal variables and events

        public SerialPortAdapter(SerialPort port)
        {
            _port = port;
        }

        /// <summary>
        /// MANDATORY setup, without it this instance will not work.
        /// </summary>
        /// <param name="settings">Configuration from AppSettings for this type of device</param>
        /// <returns>True if successfull</returns>
        public bool Setup(ICommunicationDeviceSettings settings)
        {
            if (_port is null || settings is null || !(settings is ISerialPortSettingsModel))
                return false;

            _internalSettings = (ISerialPortSettingsModel)settings;

            if (!SerialPort.GetPortNames().Contains($"COM{_internalSettings.PortNumber}"))
                return false;

            _port.PortName = $"COM{_internalSettings.PortNumber}";
            _port.BaudRate = _internalSettings.BaudRate;
            _port.DataBits = _internalSettings.DataBits;
            _port.Parity = _internalSettings.Parity;
            _port.StopBits = _internalSettings.StopBits;
            _port.ReadTimeout = _internalSettings.ReadTimeout;
            _port.WriteTimeout = _internalSettings.WriteTimeout;
            _port.Encoding = Encoding.GetEncoding(_internalSettings.Encoding);

            _port.DtrEnable = false;
            _port.RtsEnable = false;
            _port.DiscardNull = true;
            
            _sendToken = _sendTokenSource.Token;
            _receiveToken = _receiveTokenSource.Token;

            return true;
        }

        public void Open()
        {
            lock (_lock)
            {
                if (_port != null && !_port.IsOpen)
                {
                    _port.Open();
                }
            }
        }

        public void Close()
        {
            StopReceiving();
            // Close the serial port in a new thread to avoid freezes
            Task closeTask = new Task(() =>
            {
                try
                {
                    StopSending();
                    StopReceiving();
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

        /// <summary>
        /// Constantly monitors incoming communication data, filters it and generates Commands (ICommandModel)
        /// </summary>
        public async Task StartReceivingAsync()
        {
            _isLissening = true;
            string errorData = string.Empty;
            byte[] mainBuffer = new byte[4096];

            while (_port.IsOpen && !_receiveToken.IsCancellationRequested)
            {
                int bytesRead = await _port.BaseStream.ReadAsync(mainBuffer, 0, mainBuffer.Length, _receiveToken).ConfigureAwait(false);
                string rawData = Encoding.GetEncoding(_port.Encoding.CodePage).GetString(mainBuffer).Substring(0, bytesRead);

                DataReceived?.Invoke(this, rawData);
            }
            _isLissening = false;
        }

        public void StopReceiving()
        {
            _receiveTokenSource.Cancel();
            _receiveTokenSource = new CancellationTokenSource();
            _receiveToken = _receiveTokenSource.Token;
        }

        /// <summary>
        /// Sends a raw command string taken from input CommandModel to the receiving device.
        /// </summary>
        /// <param name="command">Single command to transfer</param>
        /// <returns>Task<bool> if sends or failes</bool></returns>
        public async Task<bool> SendAsync(string command)
        {
            if (string.IsNullOrEmpty(command) || _sendToken.IsCancellationRequested)
                return false;

            byte[] sendBuffer = Encoding.GetEncoding(_port.Encoding.CodePage).GetBytes(command);
            try
            {
                await _port.BaseStream.WriteAsync(sendBuffer, 0, sendBuffer.Count(), _sendToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($@"ERROR - SerialPortAdapter - SendAsync: one or more Commands could not be sent - port closed / unavailible / cancelled?");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Sends a raw command string taken from input CommandModel to the receiving device.
        /// </summary>
        /// <param name="command">Single command to transfer</param>
        /// <returns>bool if sends or failes</bool></returns>
        public bool Send(string command)
        {
            if (command is null)
                return false;

            string raw = command;

            if (!string.IsNullOrEmpty(raw))
            {
                try
                {
                    _port.Write(raw);
                    return true;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($@"ERROR - SeiralPortAdapter - Send: could not send a command - port closed / unavailible?");
                    return false;
                }
            }
            return false;
        }

        public void StopSending()
        {
            _sendTokenSource.Cancel();
            _sendTokenSource = new CancellationTokenSource();
            _sendToken = _sendTokenSource.Token;
        }

        /// <summary>
        /// Clears data in 'received' buffer
        /// </summary>
        public void DiscardInBuffer()
        {
            try
            {
                _port.DiscardInBuffer();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(@"ERROR - SerialPortAdaper - DiscardBuffer: Port was closed, nothing to discard.");
            }
        }

        public void Dispose()
        {
            _receiveTokenSource.Cancel();
            _sendTokenSource.Cancel();
            _receiveTokenSource.Dispose();
            _sendTokenSource.Dispose();
            DiscardInBuffer();
            _port.Close();

            // Added for port to close in peace, otherwise there could be a problem with opening it again.
            Thread.Sleep(100);
        }     
    }
}