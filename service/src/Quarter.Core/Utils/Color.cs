using System;
using System.Text.Json.Serialization;

namespace Quarter.Core.Utils;

public class Color
{
    public int Argb { get; }

    public static Color FromSystemColor(System.Drawing.Color sc)
        => new Color(sc.ToArgb());

    public static Color FromHexString(string hex)
    {
        var sc = System.Drawing.ColorTranslator.FromHtml(hex);
        return new Color(sc.ToArgb());
    }

    [JsonConstructor]
    public Color(int argb)
    {
        Argb = argb;
    }

    private Color()
    {
    }

    /// <summary>
    /// Get the color value in a HTML / CSS compatible string
    /// </summary>
    /// <returns>The color value as hex with a leading #</returns>
    public string ToHex()
    {
        var hex = $"{Argb:X6}";
        return $"#{hex.Substring(2)}";
    }

    public Color Darken(double percentage)
    {
        var amount = 1 - percentage;
        var c = System.Drawing.Color.FromArgb(Argb);
        var r = Math.Min(255, (int)(c.R * amount));
        var g = Math.Min(255, (int)(c.G * amount));
        var b = Math.Min(255, (int)(c.B * amount));
        var argb = System.Drawing.Color.FromArgb(c.A, r, g, b).ToArgb();
        return new Color(argb);
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        var other = (Color)obj;
        return Argb == other.Argb;
    }

    public override int GetHashCode()
    {
        return 17 * Argb;
    }

    public override string ToString()
        => ToHex();
}
