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
        private static readonly int ByteBufferSize = 4092;    // Optimal size of max single data portion received buffer
        private byte[] _buffer = new byte[ByteBufferSize];
        private static readonly int DelayValue = 1000;
        private static readonly int IntervalValue = 1000;
        private readonly SerialPort _port = new SerialPort();
        private Encoding _encoding;
        private Timer _connectionMonitor;

        private readonly object _lock = new object();
        private bool _disposed;

        public SerialPortAdapter(ICommunicationDeviceSettings settings)
        {
            Setup(settings);
            SetupPhysicallConnectionMonitoring();
        }

        public event EventHandler<string> DataReceived;

        public bool IsOpen => _port?.IsOpen == true;
        public bool IsConnected => IsPortExisting();
        public bool IsReady => IsConnected && IsOpen;
        public string Name { get; private set; }
        public int ConfirmationTimeout { get; private set; }
        public int CompletitionTimeout { get; private set; }

        public bool IsPortExisting() => SerialPort.GetPortNames().Contains(_port.PortName);

        /// <summary>
        /// Sets up all of the necessary parameters for this instance of the device
        /// </summary>
        /// <param name="settings">Configuration from AppSettings for this type of device</param>
        /// <returns>True if successful</returns>
        private void Setup(ICommunicationDeviceSettings settings)
        {
            if (!(settings is SerialPortSettings set))
                throw new ArgumentNullException(nameof(settings));

            SetUpDeviceOptions(set);
        }

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

        private void SetupPhysicallConnectionMonitoring() =>
            _connectionMonitor = new Timer(CheckPhysicallConnection, null, Timeout.Infinite, Timeout.Infinite);

        private void CheckPhysicallConnection(object state)
        {
            if (IsConnected)
                return;
            StopPhysicallConnectionMonitoring();
            throw new DeviceDisconnectedException($"Internal monitor detected that {_port.PortName} has been physically disconnected!");
        }

        private void StartPhysicallConnectionMonitoring() =>
            _connectionMonitor.Change(DelayValue, IntervalValue);

        private void StopPhysicallConnectionMonitoring() =>
            _connectionMonitor.Change(Timeout.Infinite, Timeout.Infinite);

        public void Open()
        {
            lock (_lock)
            {
                try
                {
                    if (_port.IsOpen)
                        return;

                    _port.Open();
                    StartPhysicallConnectionMonitoring();
                }
                catch (IOException ex) when (!IsConnected)
                {
                    throw new DeviceDisconnectedException($"Tried to open not connected port ({_port.PortName})", ex);
                }
            }
        }

        public void Close()
        {
            _connectionMonitor.Change(Timeout.Infinite, Timeout.Infinite);
            DiscardInBuffer();
            _port.Close();
        }

        public Task CloseAsync()
        {
            return Task.Run(() =>
            {
                StopPhysicallConnectionMonitoring();
                DiscardInBuffer();
                _port.Close();
            });
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
                if (IsConnected)
                {
                    // Only known way to handle sudden port failure AND cancellation - known SerialPort implementation hiccup.
                    throw new OperationCanceledException("System IO exception in BaseStream.ReadAsync - handled, expected, re-thrown as cancellation", ex, ct);
                }

                throw new DeviceDisconnectedException($"{_port.PortName} has been physically disconnected", ex);
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
        /// <param name="ct">Cancellation Token</param>
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

        private bool CanSend(string data) => !IsReady || string.IsNullOrEmpty(data);

        /// <summary>
        /// Clears data in 'received' buffer
        /// </summary>
        public void DiscardInBuffer()
        {
            if (IsConnected && _port.IsOpen)
            {
                _port.BaseStream.Flush();
                _port.DiscardInBuffer();
                _buffer = new byte[ByteBufferSize];
            }
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
                _port?.Dispose();

                _connectionMonitor.Change(Timeout.Infinite, Timeout.Infinite);
                _connectionMonitor?.Dispose();
            }
            _disposed = true;
        }

        #endregion IDisposable implementation
    }
}