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
