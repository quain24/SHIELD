namespace Shield.Enums
{
    /// <summary>
    /// Options for a command type when, for example, transmitting command to a device
    /// </summary>
    public enum CommandType
    {
        /// <summary>
        /// Special, empty placeholder
        /// </summary>
        Empty = 0,

        #region Message Header

        /// <summary>
        /// Ping remote device, start conversation etc.
        /// </summary>
        HandShake,

        #endregion Message Header

        #region Message type

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

        #endregion Message type

        #region Message End / footer

        /// <summary>
        /// Should be last command received, states that message is completed
        /// </summary>
        EndMessage,

        #endregion Message End / footer

        #region Message response status codes

        /// <summary>
        /// Used in responding, states that corresponding received command is known and understood
        /// </summary>
        ReceivedAsCorrect,

        /// <summary>
        /// Used in responding, states that corresponding received command was distorted beyond repair
        /// </summary>
        ReceivedAsError,

        /// <summary>
        /// Used in responding, states that corresponding received command type is not known by recipient
        /// </summary>
        ReceivedAsUnknown,

        /// <summary>
        /// Used in responding, states that corresponding received command was partial, like, for example,
        /// Data message did not contained whole data pack
        /// </summary>
        ReceivedAsPartial,

        /// <summary>
        /// Used in responding, states that message being confirmed was flagged with <see cref="Shield.HardwareCom.Models.IMessageModel.Errors"/> = <see cref="MessageErrors.ConfirmationTimeout"/> 
        /// </summary>
        ConfirmationTimeoutOccurred,

        /// <summary>
        /// Used in responding, states that message being confirmed was flagged with <see cref="Shield.HardwareCom.Models.IMessageModel.Errors"/> = <see cref="MessageErrors.CompletitionTimeout"/> 
        /// </summary>
        CompletitionTimeoutOccured,

        #endregion Message response status codes

        #region Message decoded status codes

        Error,
        Unknown,
        Partial,

        #endregion Message decoded status codes

        Confirm,
        Cancel,
        RetryLast,
        Data
    }
}