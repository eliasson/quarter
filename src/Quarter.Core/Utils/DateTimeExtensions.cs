using System;
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
}