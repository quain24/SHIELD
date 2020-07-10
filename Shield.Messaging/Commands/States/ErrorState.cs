using System;
using System.Collections.Generic;

namespace Shield.Messaging.Commands.States
{
    public sealed class ErrorState
    {
        [Flags]
        private enum StateRepresentation
        {
            Unchecked = 0,
            Valid = 1 << 0,
            BadID = 1 << 1,
            BadHostID = 1 << 2,
            TargetUnknown = 1 << 3,
            OrderDoesNotExist = 1 << 4,
            BadDataPack = 1 << 5,
            BadOrderPart = 1 << 6,
            BadTargetPart = 1 << 7
        }

        public static ErrorState Custom(string attributes)
        {
            if (Enum.TryParse(attributes, out StateRepresentation translatedRepresentation))
                return new ErrorState(translatedRepresentation);
            throw new ArgumentOutOfRangeException(nameof(attributes), $"Passed attributes ({attributes}) cannot be parsed into {nameof(ErrorState)} object.");
        }

        public static ErrorState Unchecked() => new ErrorState(StateRepresentation.Unchecked);

        private ErrorState(StateRepresentation representation) => Representation = representation;

        private StateRepresentation Representation { get; }
        private StateRepresentation Invalidate => Representation & ~StateRepresentation.Valid;

        #region IEquatable<MessageState> implementation

        public override bool Equals(object obj) => Equals(obj as ErrorState);

        public override int GetHashCode() => -1396813904 + Representation.GetHashCode();

        public bool Equals(ErrorState other)
        {
            return other != null &&
                Representation == other.Representation;
        }

        public static bool operator ==(ErrorState left, ErrorState right) => EqualityComparer<ErrorState>.Default.Equals(left, right);

        public static bool operator !=(ErrorState left, ErrorState right) => !(left == right);

        #endregion IEquatable<MessageState> implementation

        public ErrorState Valid() => new ErrorState(StateRepresentation.Valid);

        public ErrorState BadID() => new ErrorState(Invalidate | StateRepresentation.BadID);

        public ErrorState BadHostID() => new ErrorState(Invalidate | StateRepresentation.BadHostID);

        public ErrorState BadDataPack() => new ErrorState(Invalidate | StateRepresentation.BadDataPack);

        public ErrorState BadTarget() => new ErrorState(Invalidate | StateRepresentation.BadTargetPart);

        public ErrorState BadOrder() => new ErrorState(Invalidate | StateRepresentation.BadOrderPart);

        public ErrorState TargetUnknown() => new ErrorState(Invalidate | StateRepresentation.TargetUnknown);

        public ErrorState OrderDoesNotExist() => new ErrorState(Invalidate | StateRepresentation.OrderDoesNotExist);

        public override string ToString()
        {
            return Representation.ToString();
        }
    }
}