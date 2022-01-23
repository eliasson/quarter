namespace Quarter.Pages.Application.Timesheet;

public class Hour
{
    public readonly int Index;
    public QuarterViewModel[] Quarters { get; set; }
    public readonly string Label;

    public Hour(int index, QuarterViewModel[] quarters)
    {
        Index = index;
        Quarters = quarters;
        Label = index.ToString("00") + ":00";
    }
}
