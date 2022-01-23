using System;

namespace Quarter.Pages.Application.Timesheet;

public class DayLimit
{
    public int Value { get; private set; }

    private readonly bool _decrementing;
    private readonly int _limit;

    public DayLimit(int initial, int limit, bool decrementing = false)
    {
        Value = initial;
        _limit = limit;
        _decrementing = decrementing;
    }

    public bool LimitReached()
        => Value == _limit;

    public void Extend()
    {
        Value = _decrementing
            ? Math.Max(Value - 1, _limit)
            : Math.Min(Value + 1, _limit);
    }
}