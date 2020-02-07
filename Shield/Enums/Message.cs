using System;

namespace Shield.Enums
{
    /// <summary>
    /// Message transfer direction
    /// </summary>
    public enum Direction
    {
        /// <summary>
        /// Direction unknown
        /// </summary>
        Unknown,

        /// <summary>
        /// Message is incoming from remote machine
        /// </summary>
        Incoming,

        /// <summary>
        /// Outgoing direction
        /// </summary>
        Outgoing
    }

    /// <summary>
    /// Specifies type of message
    /// </summary>
    public enum MessageType
    {
        /// <summary>
        /// Type is unknown
        /// </summary>
        Unknown,

        /// <summary>
        /// Message is a confirmation of other message
        /// </summary>
        Confirmation,

        /// <summary>
        /// Message is a Master type - can be answered to.
        /// </summary>
        Master,

        /// <summary>
        /// Message is A Slave type - it is answering to a Master type from remote device
        /// </summary>
        Slave
    }

    /// <summary>
    /// Message transfer status
    /// </summary>
    public enum TransferStatus
    {
        /// <summary>
        /// Transfer status in unknown
        /// </summary>
        Unknown,

        /// <summary>
        /// Message was sent
        /// </summary>
        Sent,

        /// <summary>
        /// Message currently is being sent
        /// </summary>
        Sending,

        /// <summary>
        /// Message is awaiting to be sent
        /// </summary>
        ToBeSent,

        /// <summary>
        /// Message was received
        /// </summary>
        Received,

        /// <summary>
        /// Message is currently being received
        /// </summary>
        Receiving
    }

    /// <summary>
    /// Message confirmation status
    /// </summary>
    public enum Confirmed
    {
        /// <summary>
        /// Message was not confirmed
        /// </summary>
        No,

        /// <summary>
        /// Message was confirmed
        /// </summary>
        Yes,

        /// <summary>
        /// Unconfirmed or confirmed out of time frame
        /// </summary>

        Timeout
    }

    /// <summary>
    /// Message errors - can be multiple (flag)
    /// </summary>
    [Flags]
    public enum Errors
    {
        None = 0,
        GotPartialCommands = 1 << 0,
        GotUnknownCommands = 1 << 1,
        GotErrorCommands = 1 << 2,
        UndeterminedType = 1 << 3,
        BadMessagePattern = 1 << 4,
        NotSent = 1 << 5,
        NotConfirmed = 1 << 6,
        ConfirmationTimeout = 1 << 7,
        CompletitionTimeout = 1 << 8,
        ConfirmedNonexistent = 1 << 9,
        RespondedToNonexistent = 1 << 10,
        Empty = 1 << 11,
        WasAlreadyCompleted = 1 << 12,
        Unknown = 1 << 13,
        Incomplete = 1 << 14,
        Error = 1 << 15,
        IsNull = 1 << 16
    }
}