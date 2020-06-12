using Shield.Messaging.Devices;
using System;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Shield.COMDevice
{
    public class SerialPortAdapter : ICommunicationDeviceAsync
    {
        /// <summary>
        /// Wraps SerialPort for future use with interfaces
        /// Works on commandMNodel's, sends and receives them with translation from and into them
        /// </summary>

        private const int ByteBufferSize = 4092;    // Optimal size of single data portion received buffer
        private byte[] _buffer = new byte[ByteBufferSize];

        private readonly SerialPort _port = new SerialPort();
        private Encoding _encoding;

        private readonly object _lock = new object();
        private bool _disposed;
        private bool _wasSetupCorrectly;

        public event EventHandler<string> DataReceived;

        public bool IsOpen => _port != null && _port.IsOpen;

        public SerialPortAdapter()
        {
        }

        public SerialPortAdapter(ISerialPortSettingsModel settings) : this() => Setup(settings);

        public int ConfirmationTimeout { get; private set; }
        public int CompletitionTimeout { get; private set; }
        public string Name { get; private set; }

        /// <summary>
        /// Sets up all of the necessary parameters for this instance of the device
        /// </summary>
        /// <param name="settings">Configuration from AppSettings for this type of device</param>
        /// <returns>True if successful</returns>
        public bool Setup(ICommunicationDeviceSettings settings)
        {
            if (settings is null) throw new ArgumentNullException(nameof(settings));

            if (!WasSetupCorrectly(settings))
                return _wasSetupCorrectly = false;

            SetUpDeviceOptions((ISerialPortSettingsModel)settings);
            return _wasSetupCorrectly = true;
        }

        private bool WasSetupCorrectly(ICommunicationDeviceSettings settings)
        {
            if (_port is null || settings is null || !(settings is ISerialPortSettingsModel))
                return false;
            return PortNumberExists(((ISerialPortSettingsModel)settings).PortNumber);
        }

        private bool PortNumberExists(int portNumber) =>
            SerialPort.GetPortNames().Contains($"COM{portNumber}");

        private void SetUpDeviceOptions(ISerialPortSettingsModel settings)
        {
            if (settings is null) throw new ArgumentNullException(nameof(settings));

            _port.PortName = $"COM{settings.PortNumber}";
            _port.BaudRate = settings.BaudRate;
            _port.DataBits = settings.DataBits;
            _port.Parity = settings.Parity;
            _port.StopBits = settings.StopBits;
            _port.ReadTimeout = settings.ReadTimeout;
            _port.WriteTimeout = settings.WriteTimeout;
            _encoding = _port.Encoding = Encoding.GetEncoding(settings.Encoding);
            _port.DtrEnable = false;
            _port.RtsEnable = false;
            _port.DiscardNull = true;
            CompletitionTimeout = settings.CompletitionTimeout;
            ConfirmationTimeout = settings.ConfirmationTimeout;
            Name = settings.Name;
        }

        public void Open()
        {
            lock (_lock)
            {
                if (!_port.IsOpen && _wasSetupCorrectly)
                {
                    try
                    {
                        _port.Open();
                    }
                    catch
                    {
                        throw;
                    }
                }
            }
        }

        public void Close()
        {
            DiscardInBuffer();
            _port.Close();
        }

        public async Task CloseAsync()
        {
            await Task.Run(() =>
            {
                DiscardInBuffer();
                _port.Close();
            })
            .ConfigureAwait(false);
        }

        public string Receive()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Constantly monitors incoming communication data, filters it and generates Commands (ICommandModel)
        /// </summary>
        public async Task<string> ReceiveAsync(CancellationToken ct)
        {
            if (!_port.IsOpen)
                return string.Empty;

            try
            {
                ct.ThrowIfCancellationRequested();
                int bytesRead = await _port.BaseStream.ReadAsync(_buffer, 0, _buffer.Length, ct).ConfigureAwait(false);
                string rawData = _encoding.GetString(_buffer, 0, bytesRead);
                OnDataReceived(rawData);
                return rawData;
            }
            catch (IOException ex)
            {
                // Only known way to handle sudden port failure AND cancellation - known SerialPort implementation hiccup.
                throw new OperationCanceledException("System IO exception in BaseStream.ReadAsync - handled, expected, re-thrown. Either task was canceled or port has been closed", ex, ct);
            }
        }

        /// <summary>
        /// Sends a raw command string taken from input CommandModel to the receiving device.
        /// </summary>
        /// <param name="command">Single command to transfer</param>
        /// <returns>True if sending is successful</returns>
        public bool Send(string command)
        {
            if (!IsOpen || string.IsNullOrWhiteSpace(command))
                return false;

            try
            {
                _port.Write(command);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Sends a raw command string taken from input CommandModel to the receiving device.
        /// </summary>
        /// <param name="command">Single command to transfer</param>
        /// <returns>Task<bool> if sends or fails</returns>
        public async Task<bool> SendAsync(string command, CancellationToken ct)
        {
            if (!IsOpen || string.IsNullOrEmpty(command))
            {
                return false;
            }

            byte[] buffer = _encoding.GetBytes(command);
            try
            {
                ct.ThrowIfCancellationRequested();
                await _port.BaseStream.WriteAsync(buffer, 0, buffer.Length, ct).ConfigureAwait(false);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// Clears data in 'received' buffer
        /// </summary>
        public void DiscardInBuffer()
        {
            _port.BaseStream.Flush();
            _port.DiscardInBuffer();
            _buffer = new byte[ByteBufferSize];
        }

        protected virtual void OnDataReceived(string rawData) => DataReceived?.Invoke(this, rawData);

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
                if (_port?.IsOpen ?? false)
                    _port.Close();
            }
            _buffer = null;
            _disposed = true;
        }
    }
}