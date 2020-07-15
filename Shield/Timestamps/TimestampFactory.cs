using System;

namespace Shield.Timestamps
{
    /// <summary>
    /// <see langword="static"/> class that creates a <see cref="Timestamps.Timestamp"/> from current UTC time or from given <see cref="DateTimeOffset"/>
    /// </summary>
    public static class TimestampFactory
    {
        private static readonly HighPrecisionClock _clock = new HighPrecisionClock();

        /// <summary>
        /// Returns <see cref="Timestamps.Timestamp"/> object created from current high precision UTC DateTime
        /// </summary>
        public static Timestamp Timestamp => GetTimestamp();

        /// <summary>
        /// Creates new <see cref="Timestamps.Timestamp"/> instance from current high precision UTC DateTime
        /// </summary>
        /// <returns>New <see cref="Timestamps.Timestamp"/> instance</returns>
        public static Timestamp GetTimestamp() => new Timestamp(_clock.UtcNow.Ticks);

        /// <summary>
        /// Returns Time stamp from given DateTime
        /// </summary>
        /// <param name="dateTime">Will be converted into (long) time stamp</param>
        /// <returns>Time stamp</returns>
        public static Timestamp GetTimestamp(DateTimeOffset dateTime) => new Timestamp(dateTime.Ticks);
    }
}