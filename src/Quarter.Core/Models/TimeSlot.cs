using System;
using System.Text.Json.Serialization;
using Quarter.Core.Utils;

namespace Quarter.Core.Models;

public abstract class TimeSlot
{
    /// <summary>
    /// The offset for this quarter on a 24 hour day (i.e. between 0 and 95)
    /// </summary>
    public int Offset { get; set; }

    /// <summary>
    /// The duration in 15 minutes multiplier. I.e. duration of 2 is 30 minutes (2*15 min).
    /// </summary>
    public int Duration { get; set; }

    public UtcDateTime Created { get; protected set; }

    [JsonConstructor]
    protected TimeSlot()
    {
    }

    protected TimeSlot(int offset, int duration)
    {
        if (offset < 0) throw new ArgumentException($"Offset cannot be negative. Was {offset}");
        if (offset > 95) throw new ArgumentException($"Offset must be less than 96. Was {offset}");
        if (offset + duration > 96) throw new ArgumentException($"Range exceeds max quarters per day with {Math.Abs(96 - offset - duration)}");
        if (duration < 1) throw new ArgumentException($"Duration must be greater than zero. Was {duration}");

        Offset = offset;
        Duration = duration;
        Created = UtcDateTime.Now();
    }

    public int EndsAt
        => Offset + Duration;
}
