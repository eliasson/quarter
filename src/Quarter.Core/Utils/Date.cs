using System;
using System.Collections.Generic;

namespace Quarter.Core.Utils;

 /// <summary>
/// A date only version of a DateTime.
///
/// This class does not use any culture setting!
/// - Time is always 00:00:00 and set as UTC
/// - Start of week is always Monday
/// - End of week is always Sunday
/// </summary>
public struct Date
{
    public DateTime DateTime { get; set; }

    /// <summary>
    /// Construct a date based on the given date time.
    /// </summary>
    /// <param name="dateTime">The date</param>
    public Date(DateTime dateTime)
    {
        DateTime = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, 0, 0, 0, DateTimeKind.Utc);
    }

    /// <summary>
    /// Construct an instance of a date representing today's date
    /// </summary>
    /// <returns>Today's date</returns>
    public static Date Today()
        => new(DateTime.Now);

    /// <summary>
    /// Generate a random Date starting at 2000-01-01
    /// </summary>
    /// <returns>A date</returns>
    public static Date Random()
    {
        var random = new Random();
        var dt = new DateTime(2000, 1, 1);
        return new Date(dt.AddDays(random.Next(20 * 365) + random.Next(12) + random.Next(27)));
    }

    /// <summary>
    /// Generate a sequence of N dates starting with the given date.
    /// </summary>
    /// <param name="start">The first date in the sequence</param>
    /// <param name="count">The total number of dates to return</param>
    /// <returns>A list of dates in sequence</returns>
    /// <exception cref="ArgumentException">If date is less than 1</exception>
    public static IEnumerable<Date> Sequence(Date start, int count)
    {
        if (count < 1) throw new ArgumentException("Count must be greater than zero to return a sequence");

        var res = new List<Date>();
        for (var i = 0; i < count; i++)
            res.Add(new Date(start.DateTime.AddDays(i)));
        return res;
    }

    /// <summary>
    /// Generate a sequence of dates between the two given dates (inclusive).
    /// </summary>
    /// <param name="start">The first date in the sequence</param>
    /// <param name="end">The last date in the sequence</param>
    /// <returns>A list of dates in sequence</returns>
    /// <exception cref="ArgumentException">If end is less or equal to start</exception>
    public static IEnumerable<Date> Sequence(Date start, Date end)
    {
        if (!(end.DateTime > start.DateTime)) throw new ArgumentException("End date must be greater than start date to return a sequence");
        var span = end.DateTime - start.DateTime;
        return Sequence(start, (int) span.TotalDays + 1); // Inclusive start date
    }

    /// <summary>
    /// The date as ISO-8601 string YYYY-MM-DD
    /// </summary>
    /// <returns></returns>
    public readonly string IsoString()
        => DateTime.ToString("yyy-MM-dd");

    /// <summary>
    /// Get the date for the start of the week for the given date.
    ///
    /// NOTE: Start of the week is always a Monday!
    /// </summary>
    /// <returns>The date representing start of the week</returns>
    public Date StartOfWeek()
    {
        var dayOffset = DateTime.DayOfWeek == DayOfWeek.Sunday
            ? 6
            : (int) DateTime.DayOfWeek - 1;
        return new Date(DateTime.AddDays(-1 * dayOffset));
    }

        /// <summary>
        /// Get the date for the end of the week for the given date.
        ///
        /// NOTE: End of week is always a Sunday!
        /// </summary>
        /// <returns>The date representing end of the week</returns>
        public Date EndOfWeek()
            => new Date(StartOfWeek().DateTime.AddDays(6));
    }