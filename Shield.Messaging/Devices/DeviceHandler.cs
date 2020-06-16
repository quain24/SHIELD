using Shield.Messaging.Commands;
using Shield.Messaging.RawData;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shield.Messaging.Devices
{
    public class DeviceHandler
    {
        private readonly ICommunicationDeviceAsync _device;
        private readonly IDataStreamSplitter _streamSplitter;
        private readonly CommandFactory _commandFactory;
        private Func<Task> _listeningAction;

        private SortedDictionary<Timestamp, ICommand> _buffer = new SortedDictionary<Timestamp, ICommand>();

        public DeviceHandler(ICommunicationDeviceAsync device, IDataStreamSplitter streamSplitter, CommandFactory commandFactory)
        {
            _device = device ?? throw new ArgumentNullException(nameof(device), "Passed device cannot be NULL");
            _streamSplitter = streamSplitter ?? throw new ArgumentNullException(nameof(streamSplitter));
            _commandFactory = commandFactory ?? throw new ArgumentNullException(nameof(commandFactory));
            NotListeningState();
        }

        public async Task StartListeningAsync() => await _listeningAction().ConfigureAwait(false);

        private void ListeningState() => _listeningAction = () => Task.CompletedTask;

        private void NotListeningState() => _listeningAction = ListenAsync;

        private async Task ListenAsync()
        {
            ListeningState();

            if (!_device.IsOpen)
                _device.Open();

            while (_device.IsOpen)
            {
                string data = await ReceiveData().ConfigureAwait(false);

                foreach (var entry in Split(data))
                {
                    var command = _commandFactory.TranslateFrom(entry);
                    _buffer.Add(command.Timestamp, command);
                }
            }
            // TODO cancellation, tokens, states, events, acces to bufer etc

            NotListeningState();
        }

        private Task<string> ReceiveData() => _device.ReceiveAsync();

        private IEnumerable<RawCommand> Split(string data) => _streamSplitter.Split(data);

        private ICommand TranslateFrom(RawCommand rawCommand) => _commandFactory.TranslateFrom(rawCommand);

        private bool WasListeningCancelled(Exception e)
        {
            return e is TaskCanceledException || e is OperationCanceledException;
        }
    }
}