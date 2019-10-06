using System;
using System.Globalization;
using System.Linq;

namespace Shield.Helpers
{
    public static class IdGenerator
    {
        private const string CHARS = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        private static Random idGenerator = new Random(Guid.NewGuid().GetHashCode());
        private static string _lastId = string.Empty;

        /// <summary>
        /// Generate alpha-numeric random id of given length. Static class.
        /// </summary>
        /// <param name="length">how many chars should be generated</param>
        /// <returns></returns>
        public static string GetId(int? length)
        {
            if (length < 1 || length is null)
                return null;

            string result;

            do
            {
                result = new string(Enumerable.Range(1, (int)length).Select(A => CHARS[idGenerator.Next(CHARS.Length)]).ToArray()).ToUpper(CultureInfo.InvariantCulture);
            }
            while (result == _lastId);

            _lastId = result;

            return result;
        }
    }
}