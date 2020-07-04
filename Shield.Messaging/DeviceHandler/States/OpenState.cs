using Shield.Messaging.Commands;
using Shield.Messaging.RawData;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Shield.Messaging.DeviceHandler.States
{
    public sealed class OpenState : IDeviceHandlerState
    {
        private DeviceHandlerContext _context;
        private readonly ICommunicationDeviceAsync _device;
        private readonly IDataStreamSplitter _streamSplitter;
        private readonly CommandTranslator _commandTranslator;
        private readonly Func<ICommand, Task> _handleReceivedCommandCallbackAsync;
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();

        public OpenState(ICommunicationDeviceAsync device, IDataStreamSplitter streamSplitter, CommandTranslator commandTranslator, Func<ICommand, Task> handleReceivedCommandCallbackAsync)
        {
            _device = device;
            _streamSplitter = streamSplitter;
            _commandTranslator = commandTranslator;
            _handleReceivedCommandCallbackAsync = handleReceivedCommandCallbackAsync;
        }

        public void EnterState(DeviceHandlerContext context)
        {
            _context = context;
        }

        public void Open()
        {
            Debug.WriteLine("DeviceHandler is already open.");
        }

        public void Close()
        {
            _cts.Cancel();
            _device.Close();
            _context.SetState(new ClosedState(_device, _streamSplitter, _commandTranslator, _handleReceivedCommandCallbackAsync));
        }

        public Task StartListeningAsync()
        {
            var state = new ListeningState(_device, _streamSplitter, _commandTranslator, _handleReceivedCommandCallbackAsync);
            _context.SetState(state);
            return state.Listening();
        }

        public Task StopListeningAsync()
        {
            Debug.WriteLine("DeviceHandlerContext is not currently listening");
            return Task.CompletedTask;
        }

        public Task<bool> SendAsync(ICommand command)
        {
            return _device.SendAsync(_commandTranslator.TranslateFrom(command).ToString(), _cts.Token);
        }
    }
}