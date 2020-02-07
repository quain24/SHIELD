using System;

namespace Shield.Enums
{
    [Flags]
    public enum MessageErrors
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