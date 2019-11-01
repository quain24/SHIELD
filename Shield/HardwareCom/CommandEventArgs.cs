using Shield.HardwareCom.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shield.HardwareCom
{
    public class CommandEventArgs : EventArgs
    {
        private ICommandModel _command;

        public CommandEventArgs(ICommandModel command)
        {
            _command = command;
        }

        public ICommandModel Message { get { return _command; } }
    }
}
