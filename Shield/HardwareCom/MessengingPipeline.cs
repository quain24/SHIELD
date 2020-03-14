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
        private readonly IMessenger _messenger;
        private readonly ICommandIngester _commandIngester;
        private readonly IIncomingMessageProcessor _incomingMessageProcessor;

        public MessengingPipeline(IMessenger messanger,
                                  ICommandIngester commandIngester,
                                  IIncomingMessageProcessor incomingMessageProcessor)
        {
            _messenger = messanger ?? throw new ArgumentNullException(nameof(messanger));
            _commandIngester = commandIngester ?? throw new ArgumentNullException(nameof(commandIngester));
            _incomingMessageProcessor = incomingMessageProcessor ?? throw new ArgumentNullException(nameof(incomingMessageProcessor));

            _commandIngester.SwitchSourceCollectionTo(_messenger.GetReceivedCommands());
            _incomingMessageProcessor.SwitchSourceCollectionTo(commandIngester.GetReceivedMessages());
        }

        public void Start()
        {
            _messenger.Open();

            Task.Run(() => _messenger.StartReceiveingAsync()).ConfigureAwait(false);
            Task.Run(() => _commandIngester.StartProcessingCommands()).ConfigureAwait(false);
            Task.Run(() => _incomingMessageProcessor.StartProcessingMessagesContinous()).ConfigureAwait(false);            
            Task.Run(() => _commandIngester.StartTimeoutCheckAsync()).ConfigureAwait(false);
        }

        public void Stop()
        {
            _incomingMessageProcessor.StopProcessingMessages();
            _commandIngester.StopTimeoutCheck();
            _commandIngester.StopProcessingCommands();
            _messenger.StopReceiving();
            _messenger.Close();
        }

        public Task<bool> SendAsync(IMessageModel message) => _messenger.SendAsync(message);

        public BlockingCollection<IMessageModel> GetReceivedMessages() => _incomingMessageProcessor.GetProcessedMessages();


    }
}
