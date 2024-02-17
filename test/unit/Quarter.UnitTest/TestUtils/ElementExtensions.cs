using AngleSharp.Dom;

namespace Quarter.UnitTest.TestUtils;

public static class ElementExtensions
{
    public static string Value(this IElement elm)
        => elm.GetAttribute("value");

    public static string ButtonType(this IElement elm)
        => elm.GetAttribute("type");
}
