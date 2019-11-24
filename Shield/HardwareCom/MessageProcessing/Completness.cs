using Shield.Enums;
using Shield.HardwareCom.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shield.HardwareCom.MessageProcessing
{
    public class Completness
    {
        public bool Check(IMessageHWComModel message)
        {
            if (message is null)
                throw new ArgumentNullException(nameof(message), "Cannot pass null message!");

            if(message.Completed == true || message.Last().CommandType == CommandType.EndMessage)
                return true;

            return false;
        }
    }
}
