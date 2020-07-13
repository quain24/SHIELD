using Shield.Messaging.Commands;
using Shield.Messaging.RawData;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Shield.Messaging.DeviceHandler.States
{
    public sealed class ClosedState : IDeviceHandlerState
    {
        private DeviceHandlerContext _context;
        private Action<ICommand> _handleReceivedCommandCallback;
        private readonly CommandTranslator _commandTranslator;
        private readonly ICommunicationDeviceAsync _device;
        private readonly IDataStreamSplitter _streamSplitter;

        public ClosedState(ICommunicationDeviceAsync device, IDataStreamSplitter streamSplitter,
            CommandTranslator commandTranslator)
        {
            _device = device;
            _streamSplitter = streamSplitter;
            _commandTranslator = commandTranslator;
        }

        public void EnterState(DeviceHandlerContext context, Action<ICommand> handleReceivedCommandCallback)
        {
            _context = context;
            _handleReceivedCommandCallback = handleReceivedCommandCallback;
        }

        public void Open()
        {
            try
            {
                if (!_device.IsConnected)
                    return;
                _device.Open();
                _context.SetState(new OpenState(_device, _streamSplitter, _commandTranslator));
            }
            catch (IOException ex)
            {
                Debug.WriteLine("Could not open device");
                Debug.WriteLine(ex.Message);
                throw;
            }
        }

        public void Close()
        {
            Debug.WriteLine("DeviceHandler is already closed.");
        }

        public Task StartListeningAsync()
        {
            Debug.WriteLine("Cannot Listen because DeviceHandler is closed");
            return Task.CompletedTask;
        }

        public Task StopListeningAsync()
        {
            Debug.WriteLine("Cannot stop listening because DeviceHandler is closed");
            return Task.CompletedTask;
        }

        public Task<bool> SendAsync(ICommand command)
        {
            Debug.WriteLine("Cannot send because DeviceHandler is closed");
            return Task.FromResult(false);
        }
    }
}