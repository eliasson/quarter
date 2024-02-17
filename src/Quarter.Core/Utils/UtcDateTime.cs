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

        public string IsoString()
            => DateTime.ToString("o");

        /// <summary>
        /// Get a UTC DateTime for the current time
        /// </summary>
        /// <returns>The UTC date time</returns>
        public static UtcDateTime Now()
            => new(DateTime.UtcNow);

        /// <summary>
        /// Create a UtcDateTime instance from a date time that is known to be UTC. No control will be made.
        /// </summary>
        public static UtcDateTime FromUtcDateTime(DateTime timestamp)
            => new(new DateTime(timestamp.Year, timestamp.Month, timestamp.Day,
                timestamp.Hour, timestamp.Minute, timestamp.Second, DateTimeKind.Utc));

        /// <summary>
        /// Get a UTC DateTime representing no time / empty / null
        /// </summary>
        public static UtcDateTime MinValue
            => new(DateTime.MinValue);

        public bool Equals(UtcDateTime other)
            => DateTime.Equals(other.DateTime);

        public override bool Equals(object? obj)
            => obj is UtcDateTime other && DateTime.Equals(other.DateTime);

        public override int GetHashCode()
            => DateTime.GetHashCode();

        public static bool operator ==(UtcDateTime left, UtcDateTime right)
            => left.DateTime.Equals(right.DateTime);

        public static bool operator !=(UtcDateTime left, UtcDateTime right)
            => !left.DateTime.Equals(right.DateTime);
    }
}
