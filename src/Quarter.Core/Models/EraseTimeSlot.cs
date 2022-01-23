using System;
using System.Text.Json.Serialization;

namespace Quarter.Core.Models;

public class EraseTimeSlot : TimeSlot
{
    [JsonConstructor]
    public EraseTimeSlot() : base(0, 0)
    {
    }

    public EraseTimeSlot(int offset, int duration)
        : base(offset, duration)
    {
    }

    public override bool Equals(object? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        if (other is EraseTimeSlot otherSlot)
        {
            return Offset == otherSlot.Offset
                   && Duration == otherSlot.Duration;
        }

        return false;
    }

    public override int GetHashCode()
        => HashCode.Combine(Offset, Duration);
}