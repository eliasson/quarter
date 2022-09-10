using Quarter.Core.Utils;

namespace Quarter.Core.Queries;

public class MonthlyReportQuery
{
    public Date From { get; }
    public Date To { get; }

    public MonthlyReportQuery(Date date)
    {
        From = date.StartOfMonth();
        To = date.EndOfMonth();
    }
}

public class MonthlyReportResult
{
    public Date StartOfMonth { get; }
    public Date EndOfMonth { get; }
    public int TotalMinutes { get; }

    public MonthlyReportResult(Date startOfMonth, Date endOfMonth)
    {
        StartOfMonth = startOfMonth;
        EndOfMonth = endOfMonth;
        TotalMinutes = 0;
    }
}