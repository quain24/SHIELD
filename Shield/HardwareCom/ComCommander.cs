using Shield.HardwareCom.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shield.Enums;
using System.Collections.Concurrent;

namespace Shield.HardwareCom
{
    public class ComCommander
    {
        Func<ICommandModel> _commandFactory;
        Func<IMessageModel> _messageFactory;

        private IMessanger _messanger;
        private Dictionary<string, List<ICommandModel>> _incomingBuffer = new Dictionary<string, List<ICommandModel>>();
        private Dictionary<string, List<ICommandModel>> _outgoingBuffer = new Dictionary<string, List<ICommandModel>>();
        private Queue<IMessageModel> _incomingQueue = new Queue<IMessageModel>();
        private Queue<IMessageModel> _outgoingQueue = new Queue<IMessageModel>();        
        private IMessageModel _currentMessage;

        private bool _hasCommunicationManager = false;


        public ComCommander(Func<ICommandModel> commandFactory, Func<IMessageModel> messageFactory)
        {  
            // AutoFac auto factories
            _commandFactory = commandFactory;
            _messageFactory = messageFactory;
        }

        public void AssignMessanger(IMessanger messanger)
        {
            if(_messanger is null)
                return;
            if(_hasCommunicationManager)
                _messanger.CommandReceived -= OnCommandReceived;

            _messanger = messanger;
            _messanger.CommandReceived += OnCommandReceived;
            _hasCommunicationManager = true;
        }

        public void SendMessage(IMessageModel message)
        {
            message.IsOutgoing = true;
            _outgoingQueue.Append(message);
        }

        public void Conductor()
        {

        }

        public void Responder()
        {

        }

        public void Sender()
        {

        }

        public void Receiver(ICommandModel receivedCommand)
        {
            string id = receivedCommand.Id;           

            if (_incomingBuffer.ContainsKey(id))
            {
                _incomingBuffer[id].Append(receivedCommand);
            }

            else
            {
                _incomingBuffer.Add(id, new List<ICommandModel>{receivedCommand});
            }
        }

        public void OnCommandReceived(object sender, ICommandModel command)
        {
            Receiver(command);
        }

    }
}
