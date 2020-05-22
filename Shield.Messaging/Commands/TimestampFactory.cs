using Shield.Commands;
using System;

namespace Shield.Messaging.Commands
{
    /// <summary>
    /// Generates <see cref="Commands.Timestamp"/> from current UTC time or from given <see cref="DateTimeOffset"/>
    /// </summary>
    public static class TimestampFactory
    {
        private static readonly Clock _clock = new Clock();

        /// <summary>
        /// Returns <see cref="Commands.Timestamp"/> object created from current high precision UTC DateTime
        /// </summary>
        public static Timestamp Timestamp => GetTimestamp();

        /// <summary>
        /// Creates new <see cref="Commands.Timestamp"/> instance from current high precision UTC DateTime
        /// </summary>
        /// <returns>New <see cref="Commands.Timestamp"/> instance</returns>
        public static Timestamp GetTimestamp() => new Timestamp(_clock.UtcNow.Ticks);

        /// <summary>
        /// Returns Time stamp from given DateTime
        /// </summary>
        /// <param name="dateTime">Will be converted into (long) time stamp</param>
        /// <returns>Time stamp</returns>
        public static Timestamp GetTimestamp(DateTimeOffset dateTime) => new Timestamp(dateTime.Ticks);

        /// <summary>
        /// Calculates difference between two time stamps.
        /// returns difference or '-1' if bad parameters were given
        /// </summary>
        /// <param name="from">From this time stamp</param>
        /// <param name="to">To this time stamp</param>
        /// <returns>Difference between time stamps</returns>
    }
}