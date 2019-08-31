using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shield.Enums
{
    /// <summary>
    /// Options for a command type when, for example, transmiting command to a device
    /// </summary>
    public enum CommandType
    {
        none = -1,
        HandShake = 0,
        Confirm,
        Sending,
        StartSending,
        StopSending,
        Receiving,
        StartReceiving,
        StopReceiving,
        Completed,
        Correct,
        Error,
        Data
    }
}
