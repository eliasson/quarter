using System;
using NUnit.Framework;
using Quarter.Core.Models;
using Quarter.Core.Utils;

namespace Quarter.Core.UnitTest.Models;

[TestFixture]
public class ProjectTotalUsageTest
{
    private static readonly IdOf<Project> ProjectId = IdOf<Project>.Random();
    private static readonly IdOf<Activity> ActivityId = IdOf<Activity>.Random();
    private static readonly UtcDateTime LastUsed = new(new DateTime(2026, 11, 20, 8, 0, 0, DateTimeKind.Utc));

    private static ProjectTotalUsage Usage(
        IdOf<Project>? projectId = null,
        int totalMinutes = 60,
        int activityMinutes = 60,
        UtcDateTime? lastUsed = null)
        => new(projectId ?? ProjectId,
            totalMinutes,
            [new ActivityUsage(ActivityId, activityMinutes, LastUsed)],
            lastUsed ?? LastUsed);

    [Test]
    public void ShouldBeEqualWhenAllValuesAreEqual()
        => Assert.That(Usage(), Is.EqualTo(Usage()));

    [Test]
    public void ShouldHaveSameHashCodeWhenEqual()
        => Assert.That(Usage().GetHashCode(), Is.EqualTo(Usage().GetHashCode()));

    [Test]
    public void ShouldNotBeEqualWhenProjectDiffers()
        => Assert.That(Usage(projectId: IdOf<Project>.Random()), Is.Not.EqualTo(Usage()));

    [Test]
    public void ShouldNotBeEqualWhenTotalMinutesDiffers()
        => Assert.That(Usage(totalMinutes: 15), Is.Not.EqualTo(Usage()));

    [Test]
    public void ShouldNotBeEqualWhenActivitiesDiffer()
        => Assert.That(Usage(activityMinutes: 15), Is.Not.EqualTo(Usage()));

    [Test]
    public void ShouldNotBeEqualWhenLastUsedDiffers()
        => Assert.That(Usage(lastUsed: UtcDateTime.MinValue), Is.Not.EqualTo(Usage()));

    [Test]
    public void ShouldNotBeEqualToNull()
        => Assert.That(Usage().Equals(null), Is.False);
}
