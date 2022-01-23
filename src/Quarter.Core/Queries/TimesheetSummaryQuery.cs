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
        => new TimesheetSummaryQuery(date.StartOfWeek(), date.EndOfWeek());
}

public class TimesheetSummaryQueryResult
{
    public IList<Timesheet> Timesheets { get; } = new List<Timesheet>();

    /// <summary>
    /// The total usage in minutes
    /// </summary>
    public int TotalMinutes { get; set; }

    /// <summary>
    /// Return the total usage in hours represented by a string formatted as (hh:hh) e.g. 2.50
    /// </summary>
    /// <returns></returns>
    public string TotalAsHours()
        => ((float) TotalMinutes / 60.0).ToString("F2");

    public void Add(Timesheet timesheet)
    {
        Timesheets.Add(timesheet);
        TotalMinutes += timesheet.TotalMinutes();
    }
}