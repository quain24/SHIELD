using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Shield.Extensions;

namespace Shield.Messaging.Commands
{
    public class IdGenerator : IIdGenerator
    {
        private const string CHARS = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        private readonly Random _randomizer = new Random(Guid.NewGuid().GetHashCode());
        private readonly HashSet<string> _usedIDs = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
        private readonly ulong _bufferSize = 0;
        private readonly int _idLength = 0;
        private readonly bool _autoResetIfAllIdsUsedUp;

        /// <summary>
        /// Unique non-repeating alphanumeric random ID generator.
        /// </summary>
        /// <param name="idLength">Length of generated ID's</param>
        /// <param name="autoResetIfAllIdsUsedUp">enables automatic generator reset in case when all ID's were used up</param>
        public IdGenerator(int idLength, bool autoResetIfAllIdsUsedUp = true)
        {
            _idLength = idLength > 0
                ? idLength
                : throw new ArgumentOutOfRangeException(nameof(idLength), "Cannot create IdGenerator with id length 0 or less");
            _bufferSize = CalculateBufferSize(_idLength);
            _autoResetIfAllIdsUsedUp = autoResetIfAllIdsUsedUp;
        }

        /// <summary>
        /// Generate alpha-numeric random <c>id</c> of given length.
        /// </summary>
        /// <returns>A new ID string</returns>
        public string GetNewID()
        {
            if (AreAllIdsUsedUp())
                HandleIdBufferFull();

            string id;

            do
                id = GenerateId();
            while (IdAlreadyUsed(id));

            MarkAsUsedUp(id);
            return id;
        }

        private bool AreAllIdsUsedUp() =>
            (ulong)_usedIDs.Count >= _bufferSize;

        private void HandleIdBufferFull()
        {
            if (_autoResetIfAllIdsUsedUp)
                FlushUsedUpIdsBuffer();
            else
                throw new InvalidOperationException("Tried to get new ID but all are used up and auto reset is disabled.");
        }

        /// <summary>
        /// Clears used up ID's buffer, so they can be reused
        /// </summary>
        public void FlushUsedUpIdsBuffer() =>
            _usedIDs.Clear();

        private string GenerateId()
        {
            return new string(Enumerable
                .Range(1, _idLength)
                .Select(_ => CHARS[_randomizer.Next(CHARS.Length)]).ToArray())
                .ToUpper(CultureInfo.InvariantCulture);
        }

        private bool IdAlreadyUsed(string id) => _usedIDs.Contains(id);

        /// <summary>
        /// Add ID that was used up by, for example, incoming master message or similar,
        /// so it wont be generated later by <c>IdGenerator.GetID</c> method.
        /// </summary>
        /// <param name="ids">Used up ID</param>
        public void MarkAsUsedUp(params string[] ids)
        {
            if (ids.IsNullOrEmpty()) return;

            foreach (var entry in ids)
            {
                if (IsUsedOrEmpty(entry))
                    throw new ArgumentOutOfRangeException(nameof(ids), "One of collection values is empty or already used, so it cannot be added");

                _usedIDs.Add(entry.ToUpperInvariant());
            }
        }

        private bool IsUsedOrEmpty(string id) =>
            string.IsNullOrWhiteSpace(id) || _usedIDs.Contains(id);

        /// <summary>
        /// Get used up ID's from this instance
        /// </summary>
        /// <returns><see cref="IEnumerable{string}"/> of used up ID's</returns>
        public IEnumerable<string> GetUsedUpIds() =>
            new HashSet<string>(_usedIDs);

        private ulong CalculateBufferSize(int idLength)
        {
            var count = (ulong)CHARS.Length;

            for (ulong x = 1; x <= (ulong)idLength - 1; x++)
                count *= ((ulong)CHARS.Length - x) / x;

            return count / (ulong)idLength;
        }
    }
}