using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shield.Helpers;

//  Zawiera spis komend (command) i nimi operuje: dodaje nowe, usuwa, listuje itp
//  Tego typu obiektem będziemy operować jako głównym przy wysyłaniu / odbieraniu 
//  do urządzenia/ pliku

namespace Shield.HardwareCom.Models
{
    public class MessageModel : IMessageModel
    {
        private const int ID_LENGTH = 4;        
        
        private readonly string _messageId;

        private Dictionary<int, ICommandModel> _commands = new Dictionary<int, ICommandModel>();


        public MessageModel()
        {
            _messageId = IdGenerator.GetId(ID_LENGTH);
        }

        public bool IsBeingSent { get; private set; } = false;
        public bool IsIncoming { get; private set; } = false;
        public bool IsOutgoing { get; private set; } = false;

        public void Add(ICommandModel command)
        {
            int id;
            if (_commands.Count == 0)
                id = 0;
            else
                id = _commands.Keys.Max() + 1;
            command.Id = _messageId;
            _commands.Add(id, command);
        }

        public bool Remove(int id)
        {
            if (_commands.Remove(id))
            {
                ResortCommands(id);
                return true;
            }
            return false;
        }
        
        public bool Remove(ICommandModel command)
        {
            if(_commands.Values.Any(x => x == command))
            {
                var commandToRemove = _commands.First(cmd => cmd.Value == command);
                _commands.Remove(commandToRemove.Key);
                ResortCommands(commandToRemove.Key);
                return true;
            }
            return false;           
        }       

        public int CommandCount
        {
            get { return _commands.Count(); }
        }

        #region internal helpers
        private void ResortCommands(int fromWhichKey)
        {
            if (_commands.Count == 0 || fromWhichKey > _commands.Keys.Max())
                return;
            for (int i = fromWhichKey + 1; i < _commands.Count(); i++)
            {
                ICommandModel tmpCommand = _commands[i];
                _commands.Remove(i);
                _commands.Add(i - 1, tmpCommand);
            }
        }

        #endregion
    }
}
