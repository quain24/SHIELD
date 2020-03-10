using Shield.Extensions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Shield.Helpers
{
    public class IdGenerator : IIdGenerator
    {
        private const string CHARS = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        private readonly Random _randomizer = new Random(Guid.NewGuid().GetHashCode());
        private HashSet<string> _usedIDs = new HashSet<string>();
        private readonly ulong _bufferSize = 0;
        private readonly int _idLength = 0;

        public IdGenerator(int idLength)
        {
            _idLength = idLength > 0
                ? idLength
                : throw new ArgumentOutOfRangeException(nameof(idLength), "Cannot create IdGenerator with id length less than 0");
            _bufferSize = CalculateBufferSize(_idLength);
        }

        /// <summary>
        /// Generate alpha-numeric random <c>id</c> of given length. Static class.
        /// </summary>
        /// <param name="length">how many chars should be generated</param>
        /// <returns></returns>
        public string GetNewID()
        {
            if (AreAllIdsUsedUp())
                FlushUsedUpIdsBuffer();
            // TODO think about adding some duplicate checks.
            string result;
            do
                result = new string(Enumerable
                    .Range(1, _idLength)
                    .Select(A => CHARS[_randomizer.Next(CHARS.Length)]).ToArray())
                    .ToUpper(CultureInfo.InvariantCulture);

            while (_usedIDs.Contains(result));

            MarkAsUsedUp(result);
            return result;
        }

        private bool AreAllIdsUsedUp() =>
            (ulong)_usedIDs.Count >= _bufferSize;

        /// <summary>
        /// Clears used up ID's buffer, so they can be reused
        /// </summary>
        public void FlushUsedUpIdsBuffer() =>
            _usedIDs = new HashSet<string>();

        /// <summary>
        /// Add ID that was used up by, for example, incoming master message or similar,
        /// so it wont be generated later by <c>IdGenerator.GetID</c> method.
        /// </summary>
        /// <param name="ids">Used up ID</param>
        /// <returns></returns>
        public void MarkAsUsedUp(string[] ids)
        {
            if (ids.IsNullOrEmpty()) throw new ArgumentOutOfRangeException(nameof(ids), "Cannot add empty value to used up Id's collection");

            foreach (string id in ids)
            {
                if (IsUsedOrEmpty(id))
                    throw new ArgumentOutOfRangeException(nameof(ids), "One of collection values is empty or already used, so it cannot be added");
                else
                    _usedIDs.Add(id.ToUpperInvariant());
            }
        }

        public void MarkAsUsedUp(string id)
        {            
            if (IsUsedOrEmpty(id)) throw new ArgumentOutOfRangeException(nameof(id), "Cannot add empty or used up value to used up Id's collection");
            _usedIDs.Add(id.ToUpperInvariant());
        }

        private bool IsUsedOrEmpty(string id) => 
            String.IsNullOrWhiteSpace(id) || _usedIDs.Contains(id);

        /// <summary>
        /// Get used up ID's from this instance
        /// </summary>
        /// <returns><see cref="IEnumerable{string}"/> of used up ID's</returns>
        public IEnumerable<string> GetUsedUpIds() =>
            new HashSet<string>(_usedIDs);

        private ulong CalculateBufferSize(int idLength)
        {
            ulong count = (ulong)CHARS.Length;

            for (ulong x = 1; x <= (ulong)idLength - 1; x++)
                count = count * ((ulong)CHARS.Length - x) / x;

            return count / (ulong)idLength;
        }
    }
}