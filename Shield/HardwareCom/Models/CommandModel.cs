using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shield;
using Shield.Enums;

namespace Shield.HardwareCom.Models
{
    /// <summary>
    /// Basic type for communication with a machine - single command encapsulates a single command type or a single data 'row'
    /// </summary>
    public class CommandModel : ICommandModel
    {
        private string _data = string.Empty;
        private CommandType _command;

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
