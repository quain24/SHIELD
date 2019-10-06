using Shield.CommonInterfaces;
using Shield.Data.Models;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
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

        private const int ByteBufferSize = 4092;

        private CancellationTokenSource _receiveCTS = new CancellationTokenSource();
        private CancellationToken _receiveCT;
        private CancellationTokenSource _sendCTS = new CancellationTokenSource();
        private CancellationToken _sendCT;

        private object _lock = new object();

        private readonly SerialPort _port;

        private bool _disposed = false;
        private bool _wasSetupCorrectly = false;
        private byte[] _buffer = new byte[ByteBufferSize];

        public event EventHandler<string> DataReceived;

        #endregion Internal variables and events

        public bool IsOpen { get { if (_port != null && _port.IsOpen) return true; return false; } }

        public SerialPortAdapter() => _port = new SerialPort();
        
        public SerialPortAdapter(ISerialPortSettingsModel settings) : this() => Setup(settings);       
      

        /// <summary>
        /// Sets up all of the necessary parameters for this instance of the device
        /// </summary>
        /// <param name="settings">Configuration from AppSettings for this type of device</param>
        /// <returns>True if successfull</returns>
        public bool Setup(ICommunicationDeviceSettings settings)
        {
            if (_port is null || settings is null || !(settings is ISerialPortSettingsModel))
                return _wasSetupCorrectly = false;

            ISerialPortSettingsModel internalSettings = (ISerialPortSettingsModel)settings;

            if (!SerialPort.GetPortNames().Contains($"COM{internalSettings.PortNumber}"))
                return _wasSetupCorrectly = false;

            _port.PortName = $"COM{internalSettings.PortNumber}";
            _port.BaudRate = internalSettings.BaudRate;
            _port.DataBits = internalSettings.DataBits;
            _port.Parity = internalSettings.Parity;
            _port.StopBits = internalSettings.StopBits;
            _port.ReadTimeout = internalSettings.ReadTimeout;
            _port.WriteTimeout = internalSettings.WriteTimeout;
            _port.Encoding = Encoding.GetEncoding(internalSettings.Encoding);

            _port.DtrEnable = false;
            _port.RtsEnable = false;
            _port.DiscardNull = true;

            _sendCT = _sendCTS.Token;
            _receiveCT = _receiveCTS.Token;

            return _wasSetupCorrectly = true;
        }

        public void Open()
        {
            lock (_lock)
            {
                if (_port != null && !_port.IsOpen && _wasSetupCorrectly)
                {
                    _port.Open();
                }
            }
        }

        public void Close()
        {
            StopReceiving();
            StopSending();
            // Close the serial port in a new thread to avoid freezes
            Task closeTask = new Task(() =>
            {
                try
                {
                    DiscardInBuffer();
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
        public async Task<string> ReceiveAsync(CancellationToken cancellToken)
        {
            cancellToken.Register(() => StopReceiving());

            if (_port.IsOpen && !_receiveCT.IsCancellationRequested && !cancellToken.IsCancellationRequested)
            {
                int bytesRead = await _port.BaseStream.ReadAsync(_buffer, 0, _buffer.Length, _receiveCT).ConfigureAwait(false);
                string rawData = Encoding.GetEncoding(_port.Encoding.CodePage).GetString(_buffer).Substring(0, bytesRead);
                OnDataReceived(rawData);
                return rawData;
            }
            return null;
        }

        public void StopReceiving()
        {
            if(_receiveCTS != null)
            {
                _receiveCTS.Cancel();
                _receiveCTS = new CancellationTokenSource();
                _receiveCT = _receiveCTS.Token;
            }
        }

        /// <summary>
        /// Sends a raw command string taken from input CommandModel to the receiving device.
        /// </summary>
        /// <param name="command">Single command to transfer</param>
        /// <returns>Task<bool> if sends or failes</bool></returns>
        public async Task<bool> SendAsync(string command)
        {
            if (!IsOpen || string.IsNullOrEmpty(command) || _sendCT.IsCancellationRequested)
            {
                Debug.WriteLine($@"ERROR - SerialPortAdapter - SendAsync: Port closed / raw command empty / cancellation requested");
                return false;
            }

            byte[] buffer = Encoding.GetEncoding(_port.Encoding.CodePage).GetBytes(command);
            try
            {
                await _port.BaseStream.WriteAsync(buffer, 0, buffer.Count(), _sendCT).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($@"ERROR - SerialPortAdapter - SendAsync: one or more Commands could not be sent - port closed / unavailible / cancelled?");
                Debug.WriteLine(ex.Message);
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
            if (!IsOpen || string.IsNullOrEmpty(command))
                return false;

            try
            {
                _port.Write(command);
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($@"ERROR - SeiralPortAdapter - Send: could not send a command - port closed / unavailible?");
                return false;
            }
        }

        public void StopSending()
        {
            if(_sendCTS != null)
            {
                _sendCTS.Cancel();
                _sendCTS.Dispose();
                _sendCTS = new CancellationTokenSource();
                _sendCT = _sendCTS.Token;
            }
        }

        /// <summary>
        /// Clears data in 'received' buffer
        /// </summary>
        public void DiscardInBuffer()
        {
            try
            {
                _port.DiscardInBuffer();
                _buffer = new byte[ByteBufferSize];
            }
            catch (Exception ex)
            {
                Debug.WriteLine(@"ERROR - SerialPortAdaper - DiscardBuffer: Port was closed, nothing to discard.");
            }
        }

        protected virtual void OnDataReceived(string rawData)
        {
            DataReceived?.Invoke(this, rawData);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                if (_receiveCTS != null)
                {
                    _receiveCTS.Cancel();
                    _receiveCTS.Dispose();
                    _receiveCTS = null;
                }
                if (_sendCTS != null)
                {
                    _sendCTS.Cancel();
                    _sendCTS.Dispose();
                    _sendCTS = null;
                }
                if (_port != null)
                {
                    if (_port.IsOpen)
                        this.Close();
                    _port.Dispose();
                }
            }
            _buffer = null;

            _disposed = true;
        }
    }
}