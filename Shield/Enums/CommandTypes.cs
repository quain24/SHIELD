namespace Shield.Enums
{
    /// <summary>
    /// Options for a command type when, for example, transmiting command to a device
    /// </summary>
    public enum CommandType
    {
        /// <summary>
        /// Special, empty placeholder
        /// </summary>
        Empty = 0,

        /// <summary>
        /// Ping remote device, start conversation etc.
        /// </summary>
        HandShake,

        /// <summary>
        /// States that this message is a base message, can be responded to.
        /// </summary>
        Master,

        /// <summary>
        /// States that this message is a response to another message.
        /// </summary>
        Slave,

        /// <summary>
        /// States that this message is just a confirmation, typically followed by few <c>CommandType:Confirm</c>'s
        /// or <c>Commandtype:Error</c>. 
        /// </summary>
        Confirmation,

        /// <summary>
        /// Should be last command received, states that message is completed
        /// </summary>
        Completed,

        /// <summary>
        /// Used in responding, states that corresponding received command is known and understood
        /// </summary>
        Correct,

        Confirm,        
        Cancel,
        RetryLast,
        Error,
        Unknown,
        Partial,
        Data    
    }
}