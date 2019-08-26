using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//  Zawiera spis komend (command) i nimi operuje: dodaje nowe, usuwa, listuje itp
//  Tego typu obiektem będziemy operować jako głównym przy wysyłaniu / odbieraniu 
//  do urządzenia/ pliku

namespace Shield.HardwareCom.Models
{
    public class Message : IMessage
    {
        private Dictionary<int, Command> _commands = new Dictionary<int, Command>();

        public void Add(Command command)
        {
            int id;
            if (_commands.Count == 0)
                id = 0;
            else
                id = _commands.Keys.Max() + 1;
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
        
        public bool Remove(Command command)
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

        private void ResortCommands(int fromWhichKey)
        {
            if (_commands.Count == 0 || fromWhichKey > _commands.Keys.Max())
                return;
            for (int i = fromWhichKey + 1; i < _commands.Count(); i++)
            {
                Command tmpCommand = _commands[i];
                _commands.Remove(i);
                _commands.Add(i - 1, tmpCommand);
            }
        }

        public int CommandCount
        {
            get
            {
                return _commands.Count();
            }
        }

    }


}
