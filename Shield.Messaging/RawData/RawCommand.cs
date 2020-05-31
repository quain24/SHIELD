using System;

namespace Shield.Messaging.RawData
{
    public class RawCommand
    {
        private readonly string _rawCommand;

        public RawCommand(string rawCommand)
        {
            _rawCommand = string.IsNullOrWhiteSpace(rawCommand.Trim())
                ? throw new ArgumentOutOfRangeException(nameof(rawCommand), "RawCommand cannot be created from NULL or empty / whitespace string")
                : rawCommand;
        }

        public override string ToString() => _rawCommand;

        public int Length => _rawCommand.Length;
    }
}