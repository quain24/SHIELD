using Shield.CommonInterfaces;
using Shield.HardwareCom.CommandProcessing;
using Shield.HardwareCom.Models;
using Shield.HardwareCom.RawDataProcessing;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Shield.HardwareCom
{
    /// <summary>
    /// Basic communication class - every command sent or received goes through this class.
    /// Constantly monitors incoming communication and simultaneously sends given commands.
    /// Needs to be setup by <c>Setup</c> method.
    /// OPEN / START RECEIVING / START DECODING.
    /// Returns decoded commands by <c>CommandReceived</c> event.
    /// </summary>
    public class Messenger : IMessenger
    {
        private readonly ICommunicationDevice _device;
        private readonly ICommandTranslator _commandTranslator;
        private readonly IIncomingDataPreparer _incomingDataPreparer;

        private CancellationTokenSource _receiveCTS = new CancellationTokenSource();

        private bool _disposed = false;
        private bool _receiverRunning = false;
        private bool _isSending = false;
        private readonly object _receiverLock = new object();

        private readonly BlockingCollection<ICommandModel> _output = new BlockingCollection<ICommandModel>();

        public event EventHandler<ICommandModel> CommandReceived;

        public bool IsOpen => _device.IsOpen;
        public bool IsReceiving => _receiverRunning;
        public bool IsSending => _isSending;

        public Messenger(ICommunicationDevice device, ICommandTranslator commandTranslator, IIncomingDataPreparer incomingDataPreparer)
        {
            _device = device ?? throw new ArgumentNullException(nameof(device));
            _commandTranslator = commandTranslator ?? throw new ArgumentNullException(nameof(commandTranslator));
            _incomingDataPreparer = incomingDataPreparer ?? throw new ArgumentNullException(nameof(incomingDataPreparer));
        }

        public void Open()
        {
            if (CanOpenDevice())
                _device.Open();
        }

        private bool CanOpenDevice() => IsOpen == false;

        public void Close()
        {
            StopReceiving();
            _device?.Close();
        }

        public async Task StartReceiveingAsync(CancellationToken ct)
        {
            if (!CanStartReceiving())
                return;

            CancellationToken internalCT = SetupCancellationTokenReceiving(ct);

            try
            {
                while (_device.IsOpen)
                {
                    internalCT.ThrowIfCancellationRequested();
                    string rawData = await _device.ReceiveAsync(internalCT).ConfigureAwait(false);
                    internalCT.ThrowIfCancellationRequested();

                    List<string> rawCommands = _incomingDataPreparer.DataSearch(rawData);

                    foreach (string c in rawCommands)
                    {
                        internalCT.ThrowIfCancellationRequested();
                        ICommandModel receivedCommad = _commandTranslator.FromString(c);
                        _output.Add(receivedCommad);
                        OnCommandReceived(receivedCommad);
                    }
                }
                _receiverRunning = false;
            }
            catch (Exception e)
            {
                if (!WasStartReceivingCorrectlyCancelled(e))
                    throw;
            }
        }

        private bool CanStartReceiving()
        {
            lock (_receiverLock)
                return _receiverRunning || !IsOpen ? false : _receiverRunning = true;
        }

        private CancellationToken SetupCancellationTokenReceiving(CancellationToken passedDownToken)
        {
            CancellationToken internalCT = passedDownToken == default ? _receiveCTS.Token : passedDownToken;
            if (internalCT == passedDownToken)
                internalCT.Register(StopReceiving);
            return internalCT;
        }

        private bool WasStartReceivingCorrectlyCancelled(Exception e)
        {
            lock (_receiverLock)
                _receiverRunning = false;
            return e is TaskCanceledException || e is OperationCanceledException;
        }

        public void StopReceiving()
        {
            _receiveCTS.Cancel();
            _device.DiscardInBuffer();
            _receiveCTS.Dispose();
            _receiveCTS = new CancellationTokenSource();
        }

        public async Task<bool> SendAsync(IMessageModel message)
        {
            if (message is null)
                return false;

            var results = new List<bool>();

            foreach (ICommandModel c in message)
                results.Add(await SendAsync(c).ConfigureAwait(false));

            return results.Contains(false) ? false : true;
        }

        public async Task<bool> SendAsync(ICommandModel command)
        {
            _isSending = true;
            bool status = await _device.SendAsync(_commandTranslator.FromCommand(command)).ConfigureAwait(false);
            _isSending = false;
            return status;
        }

        public BlockingCollection<ICommandModel> GetReceivedCommands() =>
            _output;

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
                _receiveCTS?.Cancel();
                _receiveCTS?.Dispose();
                _receiveCTS = null;

                _output?.Dispose();
                _device?.Dispose();
            }

            _disposed = true;
            // free native resources if there are any.
            //if (nativeResource != IntPtr.Zero)
            //{
            //}
        }
    }
}