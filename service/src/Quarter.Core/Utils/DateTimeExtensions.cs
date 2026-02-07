using System;
using System.Collections.Generic;
using System.Globalization;

namespace Quarter.Core.Utils;

public static class DateTimeExtensions
{
    public static bool IsAdjacentMonth(this DateTime self, DateTime other)
    {
        var month = new DateTime(self.Year, self.Month, 1);
        var previousMonth = month.Month == 1
            ? new DateTime(self.Year - 1, 12, 1)
            : new DateTime(self.Year, self.Month - 1, 1);
        var nextMonth = month.Month == 12
            ? new DateTime(self.Year + 1, 1, 1)
            : new DateTime(self.Year, self.Month + 1, 1);

        return (other.Year == previousMonth.Year && other.Month == previousMonth.Month) ||
               (other.Year == nextMonth.Year && other.Month == nextMonth.Month);
    }

    public static int Iso8601WeekNumber(this DateTime self)
    {
        // https://stackoverflow.com/a/11155102
        // CC BY-SA 4.0
        var day = CultureInfo.InvariantCulture.Calendar.GetDayOfWeek(self);
        if (day >= DayOfWeek.Monday && day <= DayOfWeek.Wednesday)
            self = self.AddDays(3);
        return CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(self, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
    }

    public static string MonthAndYear(this DateTime self)
    {
        var nameOfMonth = self.ToString("MMMM", CultureInfo.InvariantCulture);
        return $"{nameOfMonth} {self.Year}";
    }

    public static DateTime WithoutMilliseconds(this DateTime self)
    {
        return new DateTime(self.Year, self.Month, self.Day, self.Hour, self.Minute, self.Second, 0,
            self.Kind);
    }

    public static DateTime LastDayOfMonth(this DateTime self)
        => self.AddMonths(1).AddDays(-1);

    /// <summary>
    /// The date as ISO-8601 string YYYY-MM-DD
    /// </summary>
    public static string IsoString(this DateTime self)
        => self.ToString("yyyy-MM-dd");

    /// <summary>
    /// Get the range of dates between the given start and end date (inclusive).
    /// </summary>
    /// <param name="self">The current date is the start date.</param>
    /// <param name="end">The date to end the range with.</param>
    /// <exception cref="ArgumentException">If the end date is earlier than the start, or if the dates are using different timezones.</exception>
    public static IEnumerable<DateTime> RangeTo(this DateTime self, DateTime end)
    {
        if (self.Kind != end.Kind)
            throw new ArgumentException("self and end must have the same DateTimeKind");

        if (end < self)
            throw new ArgumentException("end must be greater than self");

        for (var d = self; d <= end; d = d.AddDays(1))
            yield return d;
    }
}
