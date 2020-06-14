using Shield.CommonInterfaces;
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
    /// <summary>
    /// Wraps SerialPort for future use with interfaces
    /// </summary>
    public class SerialPortAdapter : ICommunicationDeviceAsync
    {
        private readonly SerialPort _port = new SerialPort();
        private Encoding _encoding;
        private const int ByteBufferSize = 4092;    // Optimal size of max single data portion received buffer
        private byte[] _buffer = new byte[ByteBufferSize];

        private readonly object _lock = new object();
        private bool _disposed;

        public event EventHandler<string> DataReceived;

        public SerialPortAdapter(ICommunicationDeviceSettings settings) => Setup(settings);

        public bool IsOpen => _port?.IsOpen == true;
        public int ConfirmationTimeout { get; private set; }
        public int CompletitionTimeout { get; private set; }
        public string Name { get; private set; }

        /// <summary>
        /// Sets up all of the necessary parameters for this instance of the device
        /// </summary>
        /// <param name="settings">Configuration from AppSettings for this type of device</param>
        /// <returns>True if successful</returns>
        public void Setup(ICommunicationDeviceSettings settings)
        {
            if (!(settings is SerialPortSettings set))
                throw new ArgumentNullException(nameof(settings));

            SetUpDeviceOptions(set);
        }

        private bool PortNumberExists(int portNumber) =>
            SerialPort.GetPortNames().Contains($"COM{portNumber}");

        private void SetUpDeviceOptions(SerialPortSettings settings)
        {
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
                if (!_port.IsOpen)
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
            if (!_port.IsOpen)
                return string.Empty;

            try
            {
                int bytesRead = _port.Read(_buffer, 0, _buffer.Length);
                string rawData = _encoding.GetString(_buffer, 0, bytesRead);
                OnDataReceived(rawData);
                return rawData;
            }
            catch (IOException)
            {
                return string.Empty; // Receiving failed - port closed or disconnected
            }
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
            catch (Exception ex) when (ex is IOException || ex is OperationCanceledException)
            {
                // Only known way to handle sudden port failure AND cancellation - known SerialPort implementation hiccup.
                throw new OperationCanceledException("System IO exception in BaseStream.ReadAsync - handled, expected, re-thrown. Either task was canceled or port has been closed", ex, ct);
            }
        }

        /// <summary>
        /// Sends a raw data string.
        /// </summary>
        /// <param name="data">Single data string to transfer</param>
        /// <returns>True if sending was successful</returns>
        public bool Send(string data)
        {
            if (!CanSend(data))
                return false;

            byte[] buffer = _encoding.GetBytes(data);
            try
            {
                _port.Write(buffer, 0, buffer.Length);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Sends a raw data string asynchronously.
        /// </summary>
        /// <param name="data">Single data string to transfer</param>
        /// <returns>True if sending was successful</returns>
        public async Task<bool> SendAsync(string data, CancellationToken ct)
        {
            if (!CanSend(data))
                return false;

            byte[] buffer = _encoding.GetBytes(data);
            try
            {
                ct.ThrowIfCancellationRequested();
                await _port.BaseStream.WriteAsync(buffer, 0, buffer.Length, ct).ConfigureAwait(false);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool CanSend(string data) => !IsOpen || string.IsNullOrEmpty(data);

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

        #region IDisposable implementation

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
            _disposed = true;
        }

        #endregion IDisposable implementation
    }
}