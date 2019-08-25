using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shield;
using Shield.HardwareCom.Enums;

namespace Shield.HardwareCom.Models
{
    public class Command : ICommand
    {
        private string _data = string.Empty;
        private CommandType _command = CommandType.none;

        public string Data
        {
            get { return _data; }
            set { _data = value; }
        }
        public CommandType CommandType {
            get
            {
                return _command;
            }
            set
            {
                _command = value;
            }
        }

        public string CommandTypeString
        {
            get
            {
                return Enum.GetName(typeof(CommandType), CommandType);
            }
        }

    }
}
