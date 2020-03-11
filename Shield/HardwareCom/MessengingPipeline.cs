using Shield.CommonInterfaces;
using Shield.HardwareCom.Factories;
using Shield.HardwareCom.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shield.HardwareCom
{
    public class MessengingPipeline
    {
        private readonly ICommunicationDevice _device;
        private readonly IMessengerFactory _messengerFactory;
        private readonly ICommandIngester _ingester;
        private readonly IIncomingMessageProcessor _incomingMessageProcessor;

        private readonly IMessenger _messenger;

        public MessengingPipeline(ICommunicationDevice device,
                                       IMessengerFactory messangerFactory,
                                       ICommandIngester ingester,
                                       IIncomingMessageProcessor incomingMessageProcessor)
        {
            _device = device ?? throw new ArgumentNullException(nameof(device));
            _messengerFactory = messangerFactory ?? throw new ArgumentNullException(nameof(messangerFactory));
            _ingester = ingester ?? throw new ArgumentNullException(nameof(ingester));
            _incomingMessageProcessor = incomingMessageProcessor ?? throw new ArgumentNullException(nameof(incomingMessageProcessor));

            _messenger = _messengerFactory.CreateMessangerUsing(device);
            _messenger.CommandReceived += OnNewCommand;

            _incomingMessageProcessor.SwitchSourceCollection(ingester.GetReceivedMessages());
        }

        private void OnNewCommand(object sender, ICommandModel command)            
        {
            _ingester.AddCommandToProcess(command);
        }

        public void Start()
        {
            _messenger.Open();

            Task.Run(() => _messenger.StartReceiveingAsync()).ConfigureAwait(false);
            Task.Run(() => _ingester.StartProcessingCommands()).ConfigureAwait(false);
            Task.Run(() => _incomingMessageProcessor.StartProcessingMessagesContinous()).ConfigureAwait(false);            
            Task.Run(() => _ingester.StartTimeoutCheckAsync()).ConfigureAwait(false);
        }

        public void Stop()
        {
            _incomingMessageProcessor.StopProcessingMessages();
            _ingester.StopTimeoutCheck();
            _ingester.StopProcessingCommands();
            _messenger.StopReceiving();
            _messenger.Close();
        }

        public Task<bool> SendAsync(IMessageModel message)
        {
            return _messenger.SendAsync(message);
        }

        public BlockingCollection<IMessageModel> GetReceivedMessages()
        {
            return _incomingMessageProcessor.GetProcessedMessages();
        }


    }
}
