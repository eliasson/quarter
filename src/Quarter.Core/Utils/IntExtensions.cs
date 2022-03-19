namespace Quarter.Core.Utils;

public static class IntExtensions
{
    /// <summary>
    /// Return minutes in hours represented by a string formatted as (hh:hh) e.g. 2.50
    /// </summary>
    public static string MinutesAsHours(this int minutes)
        => ((float) minutes / 60.0).ToString("F2");
}