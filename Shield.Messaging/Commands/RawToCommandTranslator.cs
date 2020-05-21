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
        // todo Ended here
        // Raw command Collection can be splitted into from given stream by DataStreamSplitter, so now 
        // we need to implement translation into commands
        public ICommand TranslateFrom(string rawCommand)
        {
            return null;
        }
    }
}
