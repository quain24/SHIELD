using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Shield.Helpers
{
    public static class IdGenerator
    {
        private const string CHARS = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        private static Random idGenerator = new Random(Guid.NewGuid().GetHashCode());
        private static HashSet<string> _usedIDs = new HashSet<string>();
        private static int _oldIDLength = 0;
        private static ulong _bufferSize = 0;
        private static int _idLength = 0;

        /// <summary>
        /// Generate alpha-numeric random <c>id</c> of given length. Static class.
        /// </summary>
        /// <param name="length">how many chars should be generated</param>
        /// <returns></returns>
        public static string GetID(int? length)
        {
            if (length < 1 || length is null)
                return null;

            // Get possible combinations count if length was changed
            if (_oldIDLength != length)
            {
                _bufferSize = CalculateBufferSize((int)length);
                _oldIDLength = (int)length;
            }

            string result;
            do
            {
                result = new string(Enumerable.Range(1, (int)length).Select(A => CHARS[idGenerator.Next(CHARS.Length)]).ToArray()).ToUpper(CultureInfo.InvariantCulture);
            }
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
        public static bool UsedThisID(string id)
        {
            if(string.IsNullOrEmpty(id))
                return false;

            _usedIDs.Add(id.ToUpperInvariant());
            return true;
        }

        private static ulong CalculateBufferSize(int idLength)
        {
            ulong count = (ulong)CHARS.Length;

            for (ulong x = 1; x <= (ulong)idLength - 1; x++)
            {
                count = count * ((ulong)CHARS.Length - x) / x;
            }

            return count / (ulong)idLength;
        }
    }
}