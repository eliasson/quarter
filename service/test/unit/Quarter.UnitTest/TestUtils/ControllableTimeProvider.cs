using System;

namespace Quarter.UnitTest.TestUtils;

public class ControllableTimeProvider(DateTimeOffset? now = null) : TimeProvider
{
    private DateTimeOffset _now = now ?? DateTimeOffset.UtcNow;

    public override DateTimeOffset GetUtcNow()
        => _now;

    public DateTimeOffset Advance(TimeSpan duration)
    {
        _now += duration;
        return _now;
    }
}
