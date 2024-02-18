using System.Collections.Generic;
using Quarter.Core.Models;
using Quarter.Core.Utils;

namespace Quarter.Core.Queries;

/// <summary>
/// Retrieve the timesheets for the span of dates. If no time is registered for a given date, an empty timesheet
/// is included. I.e. each date between and inclusive From and To will be represented by a timesheet.
/// </summary>
/// <param name="From">The date to start from (inclusive)</param>
/// <param name="To">The date to end sequence at (inclusive)</param>
public record TimesheetSummaryQuery(Date From, Date To)
{
    public static TimesheetSummaryQuery ForWeek(Date date)
        => new(date.StartOfWeek(), date.EndOfWeek());
}

public class TimesheetSummaryQueryResult
{
    public IList<Timesheet> Timesheets { get; } = new List<Timesheet>();

    /// <summary>
    /// The total usage in minutes
    /// </summary>
    public int TotalMinutes { get; set; }

    public void Add(Timesheet timesheet)
    {
        Timesheets.Add(timesheet);
        TotalMinutes += timesheet.TotalMinutes();
    }
}
