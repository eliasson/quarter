using Quarter.Core.Queries;

// ReSharper disable InconsistentNaming

namespace Quarter.HttpApi.Resources;

public record WeeklyReportResourceOutput(string startOfWeek, string endOfWeek, int totalMinutes, int[] weekdayTotals)
{
    public static WeeklyReportResourceOutput From(WeeklyReportResult result)
    {
        return new WeeklyReportResourceOutput(
            result.StartOfWeek.IsoString(),
            result.EndOfWeek.IsoString(),
            result.TotalMinutes,
            result.WeekdayTotals);
    }
}
