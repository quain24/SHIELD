using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shield.Enums
{
    /// <summary>
    /// Type of incoming message enum
    /// </summary>
    public enum IncomingMessageType
    {
        Master,
        Slave,
        Confirmation,
        Undetermined
    }
}
