using Shield.Messaging.Commands;
using Shield.Messaging.RawData;
using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Shield.Messaging.Devices.DeviceHandlerStates
{
    public sealed class ClosedState : IDeviceHandlerState
    {
        private readonly IDictionary _buffer;
        private readonly CommandTranslator _commandTranslator;
        private readonly ICommunicationDeviceAsync _device;
        private readonly IDataStreamSplitter _streamSplitter;
        private DeviceHandlerContext _context;

        public ClosedState(ICommunicationDeviceAsync device, IDataStreamSplitter streamSplitter,
            CommandTranslator commandTranslator, IDictionary buffer)
        {
            _device = device;
            _streamSplitter = streamSplitter;
            _commandTranslator = commandTranslator;
            _buffer = buffer;
        }

        public event EventHandler<ICommand> CommandReceived;

        public void EnterState(DeviceHandlerContext context)
        {
            _context = context;
        }

        public void Open()
        {
            try
            {
                if (!_device.IsConnected)
                    return;
                _device.Open();
                _context.SetState(new OpenState(_device, _streamSplitter, _commandTranslator, _buffer));
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