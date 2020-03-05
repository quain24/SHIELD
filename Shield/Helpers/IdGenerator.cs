using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Shield.Helpers
{
    public class IdGenerator : IIdGenerator
    {
        private const string CHARS = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        private readonly Random _idGenerator = new Random(Guid.NewGuid().GetHashCode());
        private HashSet<string> _usedIDs = new HashSet<string>();
        private ulong _bufferSize = 0;
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
            string result;
            do
                result = new string(Enumerable
                    .Range(1, _idLength)
                    .Select(A => CHARS[_idGenerator.Next(CHARS.Length)]).ToArray())
                    .ToUpper(CultureInfo.InvariantCulture);

            while (_usedIDs.Contains(result));

            _usedIDs.Add(result);

            if ((ulong)_usedIDs.Count >= _bufferSize)
                _usedIDs = new HashSet<string>();

            return result;
        }

        /// <summary>
        /// Add ID that was used up by, for example, incoming master message or similar,
        /// so it wont be generated later by <c>IdGenerator.GetID</c> method.
        /// </summary>
        /// <param name="id">Used up ID</param>
        /// <returns></returns>
        public bool MarkAsUsedUp(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentOutOfRangeException(nameof(id), "Cannot add empty value to used up Id's collection");

            if (_usedIDs.Contains(id))
                return false;

            _usedIDs.Add(id.ToUpperInvariant());
            return true;
        }

        private ulong CalculateBufferSize(int idLength)
        {
            ulong count = (ulong)CHARS.Length;

            for (ulong x = 1; x <= (ulong)idLength - 1; x++)
                count = count * ((ulong)CHARS.Length - x) / x;

            return count / (ulong)idLength;
        }
    }
}