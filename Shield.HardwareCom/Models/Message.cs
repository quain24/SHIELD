using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shield.HardwareCom.Models
{
    public class Message : IMessage
    {
        private Dictionary<int, Command> _commands = new Dictionary<int, Command>();

        public void AddCommand(Command command)
        {
            int id;
            if (_commands.Count == 0)
                id = 0;
            else
                id = _commands.Keys.Max() + 1;
            _commands.Add(id, command);
        }
        public bool RemoveCommand(int id)
        {
            if (_commands.Remove(id))
            {
                ResortCommands(id);
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
