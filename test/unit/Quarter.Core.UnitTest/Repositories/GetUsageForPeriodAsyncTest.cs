using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Quarter.Core.Models;
using Quarter.Core.Repositories;
using Quarter.Core.UnitTest.TestUtils;
using Quarter.Core.Utils;

namespace Quarter.Core.UnitTest.Repositories;

public abstract class GetUsageForPeriodAsyncTest
{
    protected abstract ITimesheetRepository Repository();

    private static readonly DateTime NowInTest = DateTime.Now;
    private readonly Date _beforeDate = new Date(NowInTest.Subtract(TimeSpan.FromDays(4)));
    private readonly Date _fromDate = new Date(NowInTest.Subtract(TimeSpan.FromDays(3)));
    private readonly Date _duringDate = new Date(NowInTest.Subtract(TimeSpan.FromDays(2)));
    private readonly Date _toDate = new Date(NowInTest.Subtract(TimeSpan.FromDays(1)));
    private readonly Date _afterDate = new Date(NowInTest);
    private readonly IdOf<Project> _projectIdA = IdOf<Project>.Random();
    private readonly IdOf<Project> _projectIdB = IdOf<Project>.Random();
    private readonly IdOf<Activity> _activityAa= IdOf<Activity>.Random();
    private readonly IdOf<Activity> _activityAb= IdOf<Activity>.Random();
    private readonly IdOf<Activity> _activityBa = IdOf<Activity>.Random();

    private ITimesheetRepository _repository;
    private UsageOverTime _usage;

    [OneTimeSetUp]
    public async Task SetupTest()
    {
        _repository = Repository();
        await _repository.Truncate(CancellationToken.None);

        await RegisterSomeTimeSlots(_beforeDate);
        await RegisterSomeTimeSlots(_fromDate);
        await RegisterSomeTimeSlots(_duringDate);
        await RegisterSomeTimeSlots(_toDate);
        await RegisterSomeTimeSlots(_afterDate);

        _usage = await _repository.GetUsageForPeriodAsync(_fromDate, _toDate, CancellationToken.None);
    }

    private async Task RegisterSomeTimeSlots(Date date)
    {
        var ts = Timesheet.CreateForDate(date);
        ts.Register(new ActivityTimeSlot(_projectIdA, _activityAa, 0, 2));
        ts.Register(new ActivityTimeSlot(_projectIdA, _activityAa, 4, 4));
        ts.Register(new ActivityTimeSlot(_projectIdA, _activityAb, 8, 4));
        ts.Register(new ActivityTimeSlot(_projectIdB, _activityBa, 20, 20));
        await _repository.CreateAsync(ts, CancellationToken.None);
    }

    [Test]
    public void ShouldNotContainUsageForBeforeDate()
        => Assert.False(_usage.Usage.ContainsKey(_beforeDate));

    [Test]
    public void ShouldNotContainUsageForAfterDate()
        => Assert.False(_usage.Usage.ContainsKey(_afterDate));

    [Test]
    public void ShouldContainUsageForFromDate()
    {
        var actualUsage = _usage.Usage[_fromDate];
        var expectedUsage = new List<ProjectTotalUsage>()
        {
            new (_projectIdA, 10 * 15,
                new List<ActivityUsage>()
                {
                    new (_activityAa, 6 * 15, UtcDateTime.MinValue),
                    new (_activityAb, 4 * 15, UtcDateTime.MinValue),
                }, UtcDateTime.MinValue),
            new (_projectIdB, 20 * 15,
                new List<ActivityUsage>()
                {
                    new (_activityBa, 20 * 15, UtcDateTime.MinValue),
                }, UtcDateTime.MinValue),
        };

        Assert.That(actualUsage, Is.EqualTo(expectedUsage));
    }

    [Test]
    public void ShouldContainUsageForDuringDate()
    {
        var actualUsage = _usage.Usage[_duringDate];
        var expectedUsage = new List<ProjectTotalUsage>()
        {
            new (_projectIdA, 10 * 15,
                new List<ActivityUsage>()
                {
                    new (_activityAa, 6 * 15, UtcDateTime.MinValue),
                    new (_activityAb, 4 * 15, UtcDateTime.MinValue),
                }, UtcDateTime.MinValue),
            new (_projectIdB, 20 * 15,
                new List<ActivityUsage>()
                {
                    new (_activityBa, 20 * 15, UtcDateTime.MinValue),
                }, UtcDateTime.MinValue),
        };

        Assert.That(actualUsage, Is.EqualTo(expectedUsage));
    }

    [Test]
    public void ShouldContainUsageToFromDate()
    {
        var actualUsage = _usage.Usage[_toDate];
        var expectedUsage = new List<ProjectTotalUsage>()
        {
            new (_projectIdA, 10 * 15,
                new List<ActivityUsage>()
                {
                    new (_activityAa, 6 * 15, UtcDateTime.MinValue),
                    new (_activityAb, 4 * 15, UtcDateTime.MinValue),
                }, UtcDateTime.MinValue),
            new (_projectIdB, 20 * 15,
                new List<ActivityUsage>()
                {
                    new (_activityBa, 20 * 15, UtcDateTime.MinValue),
                }, UtcDateTime.MinValue),
        };

        Assert.That(actualUsage, Is.EqualTo(expectedUsage));
    }

    [Test]
    public async Task ShouldReturnEmptyUsageForPeriodWhenNoTimeIsRegistered()
    {
        var repository = Repository();
        var usage = await repository.GetUsageForPeriodAsync(new Date(DateTime.MinValue), new Date(DateTime.MaxValue), CancellationToken.None);

        Assert.That(usage.Usage, Is.Empty);
    }
}


[TestFixture]
public class InMemoryGetUsageForPeriodAsyncTest : GetUsageForPeriodAsyncTest
{
    protected override ITimesheetRepository Repository()
        => new InMemoryTimesheetRepository();
}

[TestFixture]
[Category(TestCategories.DatabaseDependency)]
public class PostgresqlGetUsageForPeriodAsyncTest : GetUsageForPeriodAsyncTest
{
    protected override ITimesheetRepository Repository()
        => new PostgresTimesheetRepository(UnitTestPostgresConnectionProvider.Instance, IdOf<User>.Random());
}
