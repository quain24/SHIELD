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
        /// Returns time stamp from current utc DateTime
        /// </summary>
        public static long TimestampNow
        {
            get { return GetTimestamp(); }
        }

        /// <summary>
        /// Returns time stamp from current utc DateTime
        /// </summary>
        /// <returns>Time stamp</returns>
        public static long GetTimestamp()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }

        /// <summary>
        /// Returns Time stamp from given DateTime
        /// </summary>
        /// <param name="dateTime">Will be converted into (long) time stamp</param>
        /// <returns>Time stamp</returns>
        public static long GetTimestamp(DateTime dateTime)
        {
            return ((DateTimeOffset)dateTime).ToUnixTimeMilliseconds();
        }

        /// <summary>
        /// Calculates difference between two time stamps.
        /// returns difference or '-1' if bad parameters were given
        /// </summary>
        /// <param name="from">From this time stamp</param>
        /// <param name="to">To this time stamp</param>
        /// <returns>Difference between time stamps</returns>
        public static long Difference(long from, long to)
        {
            if(from < 0 || to < 0)
                return -1;

            if((from - to) % 1 != 0)
                return -1;

            return from < to ? (from - to) * -1 : from - to;
        }

        /// <summary>
        /// Calculates difference between now and given time stamp in milliseconds.
        /// returns difference or '-1' if given parameter was wrong;
        /// </summary>
        /// <param name="from">Calculate difference from this time stamp</param>
        /// <returns>Difference between time stamps</returns>
        public static long Difference(long from)
        {
            if(from < 0 || from % 1 != 0)
                return -1;

            return Difference(from, TimestampNow);
        }
    }
}