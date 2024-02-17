namespace Quarter.Utils
{
    /// <summary>
    /// Tab data used by <see cref="PageContext"/> etc.
    /// </summary>
    /// <param name="Title">The display title for the tab</param>
    /// <param name="Path">The anchor href for the tabs destination</param>
    public record TabData(string Title, string Path);
}
