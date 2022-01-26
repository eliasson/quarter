using System;

namespace Quarter.Core.Utils
{
    /// <summary>
    /// A date time value object that is guaranteed to be in UTC.
    /// </summary>
    public struct UtcDateTime
    {
        public DateTime DateTime { get; set; }

        /// <summary>
        /// Construct a UTC DateTime based on a system DateTime
        /// </summary>
        /// <param name="dateTime">The DateTime to use</param>
        public UtcDateTime(DateTime dateTime)
            => DateTime = dateTime.ToUniversalTime();

        /// <summary>
        /// Get a UTC DateTime for the current time
        /// </summary>
        /// <returns>The UTC date time</returns>
        public static UtcDateTime Now()
            => new (DateTime.UtcNow);

        /// <summary>
        /// Get a UTC DateTime representing no time / empty / null
        /// </summary>
        public static UtcDateTime MinValue
            => new (DateTime.MinValue);
    }
}