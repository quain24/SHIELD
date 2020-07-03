using System;
using System.Collections;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Shield.Messaging.Commands;
using Shield.Messaging.RawData;

namespace Shield.Messaging.DeviceHandler.States
{
    public sealed class OpenState : IDeviceHandlerState
    {
        private DeviceHandlerContext _context;
        private readonly ICommunicationDeviceAsync _device;
        private readonly IDataStreamSplitter _streamSplitter;
        private readonly CommandTranslator _commandTranslator;
        private readonly IDictionary _buffer;
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();

        public OpenState(ICommunicationDeviceAsync device, IDataStreamSplitter streamSplitter, CommandTranslator commandTranslator, IDictionary buffer)
        {
            _device = device;
            _streamSplitter = streamSplitter;
            _commandTranslator =  commandTranslator;
            _buffer = buffer;
        }

        public event EventHandler<ICommand> CommandReceived;

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
            _context.SetState(new ClosedState(_device, _streamSplitter, _commandTranslator, _buffer));
        }

        public Task StartListeningAsync()
        {
            var state = new ListeningState(_device, _streamSplitter, _commandTranslator, _buffer);
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
            return  _device.SendAsync(_commandTranslator.TranslateFrom(command).ToString(), _cts.Token);
        }
    }
}