using Shield.Messaging.RawData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;

namespace Shield.Messaging.Commands
{
    public class RawToCommandTranslator
    {
        private readonly char _separator;

        public RawToCommandTranslator(char separator)
        {
            _separator = separator;
        }
        public ICommand TranslateFrom(string rawCommand)
        {
            return null;
        }
    }
}
