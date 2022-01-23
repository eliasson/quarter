using Quarter.State.ViewModels;

namespace Quarter.Pages.Application.Timesheet;

public class QuarterViewModel
{
    public int Index { get; set; }

    // These colors are used if this quarter gets assigned to another activity.
    public string? TemporaryBackgroundColor { private get; set; }
    public string? TemporaryBorderColor { private get; set; }

    // If the quarter was constructed from an existing timesheet this was the initial color (if any)
    private readonly ActivityColor? _initialActivityColor;

    public string? BackgroundColor
        => TemporaryBackgroundColor ?? _initialActivityColor?.BackgroundColor;

    public string? BorderColor
        => TemporaryBorderColor ?? _initialActivityColor?.BorderColor;

    public QuarterViewModel(int index, ActivityColor? activityColor = null)
    {
        Index = index;
        _initialActivityColor = activityColor;
    }
}