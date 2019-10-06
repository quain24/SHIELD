using Shield.CommonInterfaces;
using Shield.Enums;
using Shield.HardwareCom.Factories;
using Shield.HardwareCom.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Shield.HardwareCom
{
    public class Messanger : IMessanger
    {
        private ICommunicationDeviceFactory _communicationDeviceFactory;
        private ICommunicationDevice _device;
        private ICommandTranslator _commandTranslator;
        private IIncomingDataPreparer _incomingDataPreparer;

        private CancellationTokenSource _receiveCTS = new CancellationTokenSource();
        private CancellationToken _receiveCT;

        private BlockingCollection<string> _rawDataBuffer = new BlockingCollection<string>();

        private bool _setupSuccessufl = false;
        private bool _disposed = false;
        private bool _dataExtractorRunning = false;
        private bool _receiverRunning = false;

        private object _dataExtractorLock = new object();

        public event EventHandler<ICommandModel> CommandReceived;

        public bool IsOpen { get => _device.IsOpen; }

        public Messanger(ICommunicationDeviceFactory communicationDeviceFactory, ICommandTranslator commandTranslator, IIncomingDataPreparer incomingDataPreparer)
        {
            _communicationDeviceFactory = communicationDeviceFactory;
            _commandTranslator = commandTranslator;
            _incomingDataPreparer = incomingDataPreparer;
        }

        public bool Setup(DeviceType type)
        {
            _device = _communicationDeviceFactory.Device(type);
            if (_device is null)
                return _setupSuccessufl = false;

            _receiveCT = _receiveCTS.Token;

            _device.DataReceived += OnDataReceivedInternal;

            return _setupSuccessufl = true;
        }

        public void Open()
        {
            if (_setupSuccessufl)
                _device.Open();
        }

        public void Close()
        {
            StopReceiving();
            _rawDataBuffer.Dispose();
            _rawDataBuffer = new BlockingCollection<string>();
            _device?.Close();
        }

        public async Task StartReceiveAsync()
        {
            if(_receiverRunning)
                return;
            if (_setupSuccessufl)
            {
                // Daemons, constantly running in background, hence Task.Run()'s
                await Task.Run(async () =>
                {
                    _receiverRunning = true;
                    while (_device.IsOpen && !_receiveCT.IsCancellationRequested)
                    {
                        string toAdd = await _device.ReceiveAsync(_receiveCT).ConfigureAwait(false);
                        if (!string.IsNullOrEmpty(toAdd))
                        {
                            _rawDataBuffer.Add(toAdd);
                            if (!_dataExtractorRunning)
                                // intentionally not awaited - fire and forget
                                Task.Run(async () => await DataExtractor(_receiveCT)).ConfigureAwait(false);
                        }
                    }
                }).ConfigureAwait(false);
            }
            _receiverRunning = false;
        }

        public void StopReceiving()
        {
            _receiveCTS.Cancel();
            _receiveCTS.Dispose();
            _receiveCTS = new CancellationTokenSource();
            _receiveCT = _receiveCTS.Token;
        }

        public Task<bool> SendAsync(ICommandModel command)
        {
            return _device.SendAsync(_commandTranslator.FromCommand(command));
        }

        public async Task<bool> SendAsync(IMessageModel message)
        {
            if(message is null)
                return false;

            List<bool> results = new List<bool>();

            foreach (ICommandModel c in message)
            {
                results.Add(await _device.SendAsync(_commandTranslator.FromCommand(c)).ConfigureAwait(false));
            }

            if (results.Contains(false))
                return false;
            return true;
        }

        public bool Send(IMessageModel message)
        {
            foreach (ICommandModel c in message)
            {
                if (_device.Send(_commandTranslator.FromCommand(c)))
                    continue;
                else
                    return false;
            }
            return true;
        }

        #region internal helpers

        private async Task DataExtractor(CancellationToken cancellation)
        {
            lock (_dataExtractorLock)
            {
                if (_dataExtractorRunning)
                    return;
                _dataExtractorRunning = true;
            }

            int idleCounter = 0;
            int i = 0;
            while (!cancellation.IsCancellationRequested)
            {
                if (_rawDataBuffer.Count > 0 && !cancellation.IsCancellationRequested)
                {
                    idleCounter = 0;
                    List<string> output = _incomingDataPreparer.DataSearch(_rawDataBuffer.Take());
                    foreach (string s in output)
                    {
                        ICommandModel receivedCommad = _commandTranslator.FromString(s);
                        OnCommandReceived(receivedCommad);
                        // Temporary display to console, in future - collection?
                        Console.WriteLine(receivedCommad.CommandTypeString + " " + receivedCommad.Id + " " + receivedCommad.Data + " | Received by external data searcher (" + i++ + ")");
                    }
                }
                else
                {
                    if (idleCounter++ > 10)
                        break;
                    await Task.Delay(50).ConfigureAwait(false);
                }
            }
            _dataExtractorRunning = false;
        }

        #endregion internal helpers

        public void OnDataReceivedInternal(object sender, string e)
        {
            //_rawDataBuffer.Add(e.RemoveASCIIChars());
        }

        protected virtual void OnCommandReceived(ICommandModel command)
        {
            CommandReceived?.Invoke(this, command);
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
                // free managed resources
                if (_receiveCTS != null)
                {
                    _receiveCTS.Cancel();
                    _receiveCTS.Dispose();
                    _receiveCTS = null;
                }
                if (_rawDataBuffer != null)
                {
                    _rawDataBuffer.Dispose();
                    _rawDataBuffer = null;
                }
                if (_device != null)
                {
                    _device.Dispose();
                    _device = null;
                }
            }

            _disposed = true;
            // free native resources if there are any.
            //if (nativeResource != IntPtr.Zero)
            //{
            //}
        }
    }
}