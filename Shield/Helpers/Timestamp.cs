using System;

namespace Shield.Helpers
{
    /// <summary>
    /// Generates timestamps from current utc time or from given DateTime in
    /// unix time in milliseconds.
    /// </summary>
    public static class Timestamp
    {
        /// <summary>
        /// Returns timestamp from current utc DateTime
        /// </summary>
        public static long TimestampNow
        {
            get{ return GetTimestamp();}
        }

        /// <summary>
        /// Returns timestamp from current utc DateTime
        /// </summary>
        /// <returns>Timestamp</returns>
        public static long GetTimestamp()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }

        /// <summary>
        /// Returns Timestamp from given DateTime
        /// </summary>
        /// <param name="dateTime">Will be converted into (long) timestamp</param>
        /// <returns>Timestamp</returns>
        public static long GetTimestamp(DateTime dateTime)
        {
            return ((DateTimeOffset)dateTime).ToUnixTimeMilliseconds();
        }
    }
}