using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Quarter.Core.Models;
using Quarter.Core.Utils;

namespace Quarter.Core.UnitTest.Models;

[TestFixture]
public class TimesheetTest
{
    public class WhenConstructed
    {
        private Timesheet _timesheet;
        private Date _date;
        private IdOf<Timesheet> _timesheetId;

        [OneTimeSetUp]
        public void Setup()
        {
            _timesheetId = IdOf<Timesheet>.Random();
            _date = new Date(DateTime.Parse("2021-08-09T00:00:00Z"));
            _timesheet = Timesheet.CreateForDate(_timesheetId, _date);
        }

        [Test]
        public void ItShouldHaveAnId()
            => Assert.That(_timesheet?.Id, Is.EqualTo(_timesheetId));

        [Test]
        public void ItShouldReturnZeroTotalMinutes()
            => Assert.That(_timesheet?.TotalMinutes(), Is.Zero);

        [Test]
        public void ItShouldHaveNoTimeDetails()
            => Assert.That(_timesheet?.Date.DateTime.TimeOfDay, Is.EqualTo(TimeSpan.Zero));

        [Test]
        public void ItShouldHaveAnZeroSlots()
            => Assert.That(_timesheet?.Slots(), Is.Empty);

        [Test]
        public void ItShouldHaveAnEmptySummary()
            => Assert.That(_timesheet?.Summarize(), Is.Empty);

        [Test]
        public void ItShouldNotHaveAFirstHourUsed()
            => Assert.That(_timesheet?.FirstHourInUse, Is.Null);

        [Test]
        public void ItShouldNotHaveALastHourUsed()
            => Assert.That(_timesheet?.LastHourInUse, Is.Null);
    }

    public class WhenCompared
    {
        private Timesheet _timesheetOne;
        private Timesheet _timesheetTwo;
        private Timesheet _timesheetThree;
        private Timesheet _timesheetFour;
        private Timesheet _timesheetFive;

        [OneTimeSetUp]
        public void Setup()
        {
            var timesheetId = IdOf<Timesheet>.Random();
            var projectId = IdOf<Project>.Random();
            var projectIdTwo = IdOf<Project>.Random();
            var activityIdOne = IdOf<Activity>.Random();
            var activityIdTwo = IdOf<Activity>.Random();
            var date = new Date(DateTime.Parse("2021-08-09T00:00:00Z"));

            _timesheetOne = Timesheet.CreateForDate(timesheetId, date);
            _timesheetOne.Register(new ActivityTimeSlot(projectId, activityIdOne, 0, 3));

            _timesheetTwo = Timesheet.CreateForDate(timesheetId, date);
            _timesheetTwo.Register(new ActivityTimeSlot(projectId, activityIdOne, 0, 3));

            _timesheetThree = Timesheet.CreateForDate(timesheetId, date);
            _timesheetThree.Register(new ActivityTimeSlot(projectId, activityIdOne, 0, 3));
            _timesheetThree.Register(new ActivityTimeSlot(projectIdTwo, activityIdTwo, 10, 3));
            _timesheetThree.Register(new ActivityTimeSlot(projectId, activityIdTwo, 20, 3));

            _timesheetFour = Timesheet.CreateForDate(timesheetId, new Date(DateTime.Parse("2021-08-08T00:00:00Z")));
            _timesheetFour.Register(new ActivityTimeSlot(projectId, activityIdOne, 0, 3));

            _timesheetFive = Timesheet.CreateForDate(IdOf<Timesheet>.Random(), date);
            _timesheetFive.Register(new ActivityTimeSlot(projectId, activityIdOne, 0, 3));
        }

        [Test]
        public void ItShouldBeEqual()
            => Assert.That(_timesheetOne, Is.EqualTo(_timesheetTwo));

        [Test]
        public void ItShouldNotBeEqualWhenSlotsAreDifferent()
            => Assert.That(_timesheetOne, Is.Not.EqualTo(_timesheetThree));

        [Test]
        public void ItShouldNotBeEqualWhenDateIsDifferent()
            => Assert.That(_timesheetOne, Is.Not.EqualTo(_timesheetFour));

        [Test]
        public void ItShouldNotBeEqualWhenIdIsDifferent()
            => Assert.That(_timesheetOne, Is.Not.EqualTo(_timesheetFive));
    }

    public class WhenRegisteringMultipleActivities
    {
        private TestContext _tc;

        [OneTimeSetUp]
        public void Setup()
        {
            _tc = new TestContext();
            _tc?.Timesheet.Register(new ActivityTimeSlot(_tc.ProjectId, _tc.ActivityIdOne, 0, 3));
            _tc?.Timesheet.Register(new ActivityTimeSlot(_tc.ProjectId, _tc.ActivityIdOne, 10, 3));
        }

        [Test]
        public void ItShouldBeReflectedInTotalMinutes()
            => Assert.That(_tc?.Timesheet.TotalMinutes(), Is.EqualTo(90));

        [Test]
        public void ItShouldHaveRegisteredSlots()
            => Assert.That(_tc?.Slots, Is.EqualTo(new[]
                {
                        (_activityIdOne: _tc?.ActivityIdOne, 0, 3),
                        (_activityIdOne: _tc?.ActivityIdOne, 10, 3)
                }));

        [Test]
        public void ShouldSummarize()
        {
            var summary = _tc!.Timesheet.Summarize()
                .Select(p =>
                {
                    return (p.ProjectId, p.Duration,
                    p.Activities.Select(a => (a.ActivityId, a.Duration)).ToArray());
                });

            Assert.That(summary, Is.EqualTo(new[]
            {
                    (_projectId: _tc.ProjectId, 6, new []
                    {
                        (_activityIdOne: _tc.ActivityIdOne, 6)
                    })
                }));
        }

        [Test]
        public void ItShouldReturnTheFirstHourUsed()
            => Assert.That(_tc?.Timesheet.FirstHourInUse, Is.EqualTo(0));

        [Test]
        public void ItShouldReturnTheLastHourUsed()
            => Assert.That(_tc?.Timesheet.LastHourInUse, Is.EqualTo(3));

        [Test]
        public void ItShouldHaveACreatedTimestamp()
        {
            var now = DateTime.UtcNow;
            var created = _tc?.Timesheet.Slots().Select(s => s.Created).First();

            Assert.That(created?.DateTime - now, Is.LessThan(TimeSpan.FromMilliseconds(500)));
        }
    }

    public class WhenRegisteringAdjacentTailActivitiesWithSameActivityId
    {
        private TestContext _tc;

        [OneTimeSetUp]
        public void Setup()
        {
            _tc = new TestContext();
            _tc?.Timesheet.Register(new ActivityTimeSlot(_tc.ProjectId, _tc.ActivityIdOne, 8, 2));
            _tc?.Timesheet.Register(new ActivityTimeSlot(_tc.ProjectId, _tc.ActivityIdOne, 10, 8));
            _tc?.Timesheet.Register(new ActivityTimeSlot(_tc.ProjectId, _tc.ActivityIdTwo, 18, 4));
        }

        [Test]
        public void ItShouldBeReflectedInTotalMinutes()
            => Assert.That(_tc?.Timesheet.TotalMinutes(), Is.EqualTo(10 * 15 + 4 * 15));

        [Test]
        public void ItShouldHaveMergedAdjacentSlots()
            => Assert.That(_tc?.Slots, Is.EqualTo(new[]
                {
                        (_tc?.ActivityIdOne, 8, 10),
                        (_tc?.ActivityIdTwo, 18, 4)
                }));

        [Test]
        public void ItShouldSummarize()
        {
            var summary = _tc!.Timesheet
                .Summarize()
                .Select(p =>
                {
                    return (p.ProjectId, p.Duration,
                        p.Activities.Select(a => (a.ActivityId, a.Duration)).ToArray());
                });

            Assert.That(summary, Is.EqualTo(new[]
            {
                    (_projectId: _tc.ProjectId, 14, new []
                    {
                        (_tc.ActivityIdOne, 10),
                        (_tc.ActivityIdTwo, 4)
                    })
                }));
        }
    }

    public class WhenRegisteringAdjacentTailActivitiesWithDifferentActivityId
    {
        private TestContext _tc;

        [OneTimeSetUp]
        public void Setup()
        {
            _tc = new TestContext();
            _tc?.Timesheet.Register(new ActivityTimeSlot(_tc.ProjectId, _tc.ActivityIdOne, 8, 2));
            _tc?.Timesheet.Register(new ActivityTimeSlot(_tc.ProjectId, _tc.ActivityIdTwo, 10, 6));
            _tc?.Timesheet.Register(new ActivityTimeSlot(_tc.ProjectId, _tc.ActivityIdTwo, 18, 4));
        }

        [Test]
        public void ItShouldBeReflectedInTotalMinutes()
            => Assert.That(_tc?.Timesheet.TotalMinutes(), Is.EqualTo(2 * 15 + 6 * 15 + 4 * 15));

        [Test]
        public void ItShouldHaveMergedAdjacentSlots()
            => Assert.That(_tc?.Slots, Is.EqualTo(new[]
            {
                    (_tc?.ActivityIdOne, 8, 2),
                    (_tc?.ActivityIdTwo, 10, 6),
                    (_tc?.ActivityIdTwo, 18, 4)
            }));

        [Test]
        public void ItShouldSummarize()
        {
            var summary = _tc!.Timesheet.Summarize()
                .Select(p =>
                {
                    return (p.ProjectId, p.Duration,
                        p.Activities.Select(a => (a.ActivityId, a.Duration)).ToArray());
                });

            Assert.That(summary, Is.EqualTo(new[]
            {
                    (_projectId: _tc.ProjectId, 12, new []
                    {
                        (_tc.ActivityIdOne, 2),
                        (_tc.ActivityIdTwo, 10)
                    })
                }));
        }
    }

    public class WhenRegisteringAdjacentHeadActivitiesWithSameActivityId
    {
        private TestContext _tc;

        [OneTimeSetUp]
        public void Setup()
        {
            _tc = new TestContext();
            _tc?.Timesheet.Register(new ActivityTimeSlot(_tc.ProjectId, _tc.ActivityIdOne, 10, 8));
            _tc?.Timesheet.Register(new ActivityTimeSlot(_tc.ProjectId, _tc.ActivityIdOne, 8, 2));
            _tc?.Timesheet.Register(new ActivityTimeSlot(_tc.ProjectId, _tc.ActivityIdTwo, 18, 4));
        }

        [Test]
        public void ItShouldBeReflectedInTotalMinutes()
            => Assert.That(_tc?.Timesheet.TotalMinutes(), Is.EqualTo(10 * 15 + 4 * 15));

        [Test]
        public void ItShouldHaveMergedAdjacentSlots()
            => Assert.That(_tc?.Slots, Is.EqualTo(new[]
            {
                    (_tc?.ActivityIdOne, 8, 10),
                    (_tc?.ActivityIdTwo, 18, 4)
            }));

        [Test]
        public void ItShouldSummarize()
        {
            var summary = _tc!.Timesheet.Summarize()
                .Select(p =>
                {
                    return (p.ProjectId, p.Duration,
                        p.Activities.Select(a => (a.ActivityId, a.Duration)).ToArray());
                });

            Assert.That(summary, Is.EqualTo(new[]
            {
                    (_projectId: _tc.ProjectId, 14, new []
                    {
                        (_tc.ActivityIdOne, 10),
                        (_tc.ActivityIdTwo, 4),
                    })
                }));
        }
    }

    public class WhenRegisteringAdjacentHeadActivitiesWithDifferentActivityId
    {
        private TestContext _tc;

        [OneTimeSetUp]
        public void Setup()
        {
            _tc = new TestContext();
            _tc?.Timesheet.Register(new ActivityTimeSlot(_tc.ProjectId, _tc.ActivityIdOne, 10, 8));
            _tc?.Timesheet.Register(new ActivityTimeSlot(_tc.ProjectId, _tc.ActivityIdTwo, 8, 2));
            _tc?.Timesheet.Register(new ActivityTimeSlot(_tc.ProjectId, _tc.ActivityIdTwo, 18, 4));
        }

        [Test]
        public void ItShouldBeReflectedInTotalMinutes()
            => Assert.That(_tc?.Timesheet.TotalMinutes(), Is.EqualTo(8 * 15 + 2 * 15 + 4 * 15));

        [Test]
        public void ItShouldHaveMergedAdjacentSlots()
            => Assert.That(_tc?.Slots, Is.EqualTo(new[]
            {
                    (_tc?.ActivityIdTwo, 8, 2),
                    (_tc?.ActivityIdOne, 10, 8),
                    (_tc?.ActivityIdTwo, 18, 4)
            }));

        [Test]
        public void ItShouldSummarize()
        {
            var summary = _tc!.Timesheet.Summarize()
                .Select(p =>
                {
                    return (p.ProjectId, p.Duration,
                        p.Activities.Select(a => (a.ActivityId, a.Duration)).ToArray());
                });

            Assert.That(summary, Is.EqualTo(new[]
            {
                    (_projectId: _tc.ProjectId, 14, new []
                    {
                        (_tc.ActivityIdTwo, 6),
                        (_tc.ActivityIdOne, 8),
                    })
                }));
        }
    }

    public class WhenRegisteringAdjacentHeadAndTailActivitiesWithSameActivityId
    {
        private TestContext _tc;

        [OneTimeSetUp]
        public void Setup()
        {
            _tc = new TestContext();
            _tc?.Timesheet.Register(new ActivityTimeSlot(_tc.ProjectId, _tc.ActivityIdOne, 2, 2));
            _tc?.Timesheet.Register(new ActivityTimeSlot(_tc.ProjectId, _tc.ActivityIdOne, 6, 2));
            _tc?.Timesheet.Register(new ActivityTimeSlot(_tc.ProjectId, _tc.ActivityIdOne, 4, 2));
            _tc?.Timesheet.Register(new ActivityTimeSlot(_tc.ProjectId, _tc.ActivityIdTwo, 18, 4));
        }

        [Test]
        public void ItShouldBeReflectedInTotalMinutes()
            => Assert.That(_tc?.Timesheet.TotalMinutes(), Is.EqualTo(2 * 15 + 2 * 15 + 2 * 15 + 4 * 15));

        [Test]
        public void ItShouldHaveMergedAdjacentSlots()
            => Assert.That(_tc?.Slots, Is.EqualTo(new[]
                {
                        (_tc?.ActivityIdOne, 2, 6),
                        (_tc?.ActivityIdTwo, 18, 4)
                }));
        [Test]
        public void ItShouldSummarize()
        {
            var summary = _tc!.Timesheet.Summarize()
                .Select(p =>
                {
                    return (p.ProjectId, p.Duration,
                        p.Activities.Select(a => (a.ActivityId, a.Duration)).ToArray());
                });

            Assert.That(summary, Is.EqualTo(new[]
            {
                    (_projectId: _tc.ProjectId, 10, new []
                    {
                        (_tc.ActivityIdOne, 6),
                        (_tc.ActivityIdTwo, 4)
                    })
                }));
        }
    }

    public class WhenRegisteringAdjacentHeadAndTailActivitiesWithDifferentActivityId
    {
        private TestContext _tc;

        [OneTimeSetUp]
        public void Setup()
        {
            _tc = new TestContext();
            _tc?.Timesheet.Register(new ActivityTimeSlot(_tc.ProjectId, _tc.ActivityIdOne, 2, 2));
            _tc?.Timesheet.Register(new ActivityTimeSlot(_tc.ProjectId, _tc.ActivityIdOne, 6, 2));
            _tc?.Timesheet.Register(new ActivityTimeSlot(_tc.ProjectId, _tc.ActivityIdTwo, 4, 2));
            _tc?.Timesheet.Register(new ActivityTimeSlot(_tc.ProjectId, _tc.ActivityIdTwo, 18, 4));
        }

        [Test]
        public void ItShouldBeReflectedInTotalMinutes()
            => Assert.That(_tc?.Timesheet.TotalMinutes(), Is.EqualTo(2 * 15 + 2 * 15 + 2 * 15 + 4 * 15));

        [Test]
        public void ItShouldHaveMergedAdjacentSlots()
            => Assert.That(_tc?.Slots, Is.EqualTo(new[]
            {
                    (_tc?.ActivityIdOne, 2, 2),
                    (_tc?.ActivityIdTwo, 4, 2),
                    (_tc?.ActivityIdOne, 6, 2),
                    (_tc?.ActivityIdTwo, 18, 4)
            }));

        [Test]
        public void ItShouldSummarize()
        {
            var summary = _tc!.Timesheet.Summarize()
                .Select(p =>
                {
                    return (p.ProjectId, p.Duration,
                        p.Activities.Select(a => (a.ActivityId, a.Duration)).ToArray());
                });

            Assert.That(summary, Is.EqualTo(new[]
            {
                    (_projectId: _tc.ProjectId, 10, new []
                    {
                        (_tc.ActivityIdOne, 4),
                        (_tc.ActivityIdTwo, 6),
                    })
                }));
        }
    }

    public class WhenRegisteringOverlappingTailActivitiesWithSameActivityId
    {
        private TestContext _tc;

        [OneTimeSetUp]
        public void Setup()
        {
            _tc = new TestContext();
            _tc?.Timesheet.Register(new ActivityTimeSlot(_tc.ProjectId, _tc.ActivityIdOne, 8, 2));
            _tc?.Timesheet.Register(new ActivityTimeSlot(_tc.ProjectId, _tc.ActivityIdTwo, 10, 8));
            _tc?.Timesheet.Register(new ActivityTimeSlot(_tc.ProjectId, _tc.ActivityIdTwo, 16, 4));
        }

        [Test]
        public void ItShouldBeReflectedInTotalMinutes()
            => Assert.That(_tc?.Timesheet.TotalMinutes(), Is.EqualTo(2 * 15 + 10 * 15));

        [Test]
        public void ItShouldHaveMergedOverlappingSlots()
            => Assert.That(_tc?.Slots, Is.EqualTo(new[]
                {
                        (_tc?.ActivityIdOne, 8, 2),
                        (_tc?.ActivityIdTwo, 10, 10)
                }));

        [Test]
        public void ItShouldSummarize()
        {
            var summary = _tc!.Timesheet.Summarize()
                .Select(p =>
                {
                    return (p.ProjectId, p.Duration,
                        p.Activities.Select(a => (a.ActivityId, a.Duration)).ToArray());
                });

            Assert.That(summary, Is.EqualTo(new[]
            {
                    (_projectId: _tc.ProjectId, 12, new []
                    {
                        (_tc.ActivityIdOne, 2),
                        (_tc.ActivityIdTwo, 10),
                    })
                }));
        }
    }

    public class WhenRegisteringOverlappingTailActivitiesWithDifferentActivityId
    {
        private TestContext _tc;

        [OneTimeSetUp]
        public void Setup()
        {
            _tc = new TestContext();
            _tc?.Timesheet.Register(new ActivityTimeSlot(_tc.ProjectId, _tc.ActivityIdOne, 0, 2));
            _tc?.Timesheet.Register(new ActivityTimeSlot(_tc.ProjectId, _tc.ActivityIdOne, 10, 8));
            _tc?.Timesheet.Register(new ActivityTimeSlot(_tc.ProjectId, _tc.ActivityIdTwo, 16, 4));
        }

        [Test]
        public void ItShouldBeReflectedInTotalMinutes()
            => Assert.That(_tc?.Timesheet.TotalMinutes(), Is.EqualTo(2 * 15 + 6 * 15 + 4 * 15));

        [Test]
        public void ItShouldShrinkOverlappedSlot()
            => Assert.That(_tc?.Slots, Is.EqualTo(new[]
            {
                    (_tc?.ActivityIdOne, 0, 2),
                    (_tc?.ActivityIdOne, 10, 6),
                    (_tc?.ActivityIdTwo, 16, 4)
            }));

        [Test]
        public void ItShouldSummarize()
        {
            var summary = _tc!.Timesheet.Summarize()
                .Select(p =>
                {
                    return (p.ProjectId, p.Duration,
                        p.Activities.Select(a => (a.ActivityId, a.Duration)).ToArray());
                });

            Assert.That(summary, Is.EqualTo(new[]
            {
                    (_projectId: _tc.ProjectId, 12, new []
                    {
                        (_tc.ActivityIdOne, 8),
                        (_tc.ActivityIdTwo, 4),
                    })
                }));
        }
    }

    public class WhenRegisteringOverlappingHeadActivitiesWithSameActivityId
    {
        private TestContext _tc;

        [OneTimeSetUp]
        public void Setup()
        {
            _tc = new TestContext();
            _tc?.Timesheet.Register(new ActivityTimeSlot(_tc.ProjectId, _tc.ActivityIdOne, 8, 2));
            _tc?.Timesheet.Register(new ActivityTimeSlot(_tc.ProjectId, _tc.ActivityIdTwo, 16, 4));
            _tc?.Timesheet.Register(new ActivityTimeSlot(_tc.ProjectId, _tc.ActivityIdTwo, 10, 8));
        }

        [Test]
        public void ItShouldBeReflectedInTotalMinutes()
            => Assert.That(_tc?.Timesheet.TotalMinutes(), Is.EqualTo(2 * 15 + 10 * 15));

        [Test]
        public void ItShouldHaveMergedOverlappingSlots()
            => Assert.That(_tc?.Slots, Is.EqualTo(new[]
                {
                        (_tc?.ActivityIdOne, 8, 2),
                        (_tc?.ActivityIdTwo, 10, 10)
                }));

        [Test]
        public void ItShouldSummarize()
        {
            var summary = _tc!.Timesheet.Summarize()
                .Select(p =>
                {
                    return (p.ProjectId, p.Duration,
                        p.Activities.Select(a => (a.ActivityId, a.Duration)).ToArray());
                });

            Assert.That(summary, Is.EqualTo(new[]
            {
                    (_projectId: _tc.ProjectId, 12, new []
                    {
                        (_tc.ActivityIdOne, 2),
                        (_tc.ActivityIdTwo, 10),
                    })
                }));
        }
    }

    public class WhenRegisteringOverlappingHeadActivitiesWithDifferentActivityId
    {
        private TestContext _tc;

        [OneTimeSetUp]
        public void Setup()
        {
            _tc = new TestContext();
            _tc?.Timesheet.Register(new ActivityTimeSlot(_tc.ProjectId, _tc.ActivityIdOne, 0, 2));
            _tc?.Timesheet.Register(new ActivityTimeSlot(_tc.ProjectId, _tc.ActivityIdTwo, 16, 4));
            _tc?.Timesheet.Register(new ActivityTimeSlot(_tc.ProjectId, _tc.ActivityIdOne, 10, 8));
        }

        [Test]
        public void ItShouldBeReflectedInTotalMinutes()
            => Assert.That(_tc?.Timesheet.TotalMinutes(), Is.EqualTo(2 * 15 + 8 * 15 + 2 * 15));

        [Test]
        public void ItShouldShrinkOverlappedSlot()
            => Assert.That(_tc?.Slots, Is.EqualTo(new[]
            {
                    (_tc?.ActivityIdOne, 0, 2),
                    (_tc?.ActivityIdOne, 10, 8),
                    (_tc?.ActivityIdTwo, 18, 2)
            }));

        [Test]
        public void ItShouldSummarize()
        {
            var summary = _tc!.Timesheet.Summarize()
                .Select(p =>
                {
                    return (p.ProjectId, p.Duration,
                        p.Activities.Select(a => (a.ActivityId, a.Duration)).ToArray());
                });

            Assert.That(summary, Is.EqualTo(new[]
            {
                    (_projectId: _tc.ProjectId, 12, new []
                    {
                        (_tc.ActivityIdOne, 10),
                        (_tc.ActivityIdTwo, 2),
                    })
                }));
        }
    }

    public class WhenRegisteringASubsetSlotWithSameActivityId
    {
        private TestContext _tc;

        [OneTimeSetUp]
        public void Setup()
        {
            _tc = new TestContext();
            _tc?.Timesheet.Register(new ActivityTimeSlot(_tc.ProjectId, _tc.ActivityIdOne, 0, 8));
            _tc?.Timesheet.Register(new ActivityTimeSlot(_tc.ProjectId, _tc.ActivityIdOne, 2, 4));
        }

        [Test]
        public void ItShouldBeReflectedInTotalMinutes()
            => Assert.That(_tc?.Timesheet.TotalMinutes(), Is.EqualTo(8 * 15));

        [Test]
        public void ItShouldHaveMergedOverlappingSlots()
            => Assert.That(_tc?.Slots, Is.EqualTo(new[]
            {
                    (_activityIdOne: _tc?.ActivityIdOne, 0, 8),
            }));

        [Test]
        public void ItShouldSummarize()
        {
            var summary = _tc!.Timesheet.Summarize()
                .Select(p =>
                {
                    return (p.ProjectId, p.Duration,
                        p.Activities.Select(a => (a.ActivityId, a.Duration)).ToArray());
                });

            Assert.That(summary, Is.EqualTo(new[]
            {
                    (_projectId: _tc.ProjectId, 8, new []
                    {
                        (_activityIdOne: _tc.ActivityIdOne, 8),
                    })
                }));
        }
    }

    public class WhenRegisteringASubsetSlotWithDifferentActivityId
    {
        private TestContext _tc;

        [OneTimeSetUp]
        public void Setup()
        {
            _tc = new TestContext();
            _tc?.Timesheet.Register(new ActivityTimeSlot(_tc.ProjectId, _tc.ActivityIdOne, 0, 8));
            _tc?.Timesheet.Register(new ActivityTimeSlot(_tc.ProjectId, _tc.ActivityIdTwo, 2, 4));
        }

        [Test]
        public void ItShouldBeReflectedInTotalMinutes()
            => Assert.That(_tc?.Timesheet.TotalMinutes(), Is.EqualTo(8 * 15));

        [Test]
        public void ItShouldHaveSlitOverlappedSlot()
            => Assert.That(_tc?.Slots, Is.EqualTo(new[]
            {
                    (_tc?.ActivityIdOne, 0, 2),
                    (_tc?.ActivityIdTwo, 2, 4),
                    (_tc?.ActivityIdOne, 6, 2),
            }));

        [Test]
        public void ItShouldSummarize()
        {
            var summary = _tc!.Timesheet.Summarize()
                .Select(p =>
                {
                    return (p.ProjectId, p.Duration,
                        p.Activities.Select(a => (a.ActivityId, a.Duration)).ToArray());
                });

            Assert.That(summary, Is.EqualTo(new[]
            {
                    (_projectId: _tc.ProjectId, 8, new []
                    {
                        (_tc.ActivityIdOne, 4),
                        (_tc.ActivityIdTwo, 4),
                    })
                }));
        }
    }

    public class WhenRegisteringReplacingActivities
    {
        private TestContext _tc;

        [OneTimeSetUp]
        public void Setup()
        {
            _tc = new TestContext();
            _tc?.Timesheet.Register(new ActivityTimeSlot(_tc.ProjectId, _tc.ActivityIdOne, 8, 2));
            _tc?.Timesheet.Register(new ActivityTimeSlot(_tc.ProjectId, _tc.ActivityIdOne, 10, 8));
            _tc?.Timesheet.Register(new ActivityTimeSlot(_tc.ProjectId, _tc.ActivityIdTwo, 0, 96));
        }

        [Test]
        public void ItShouldBeReflectedInTotalMinutes()
            => Assert.That(_tc?.Timesheet.TotalMinutes(), Is.EqualTo(96 * 15));

        [Test]
        public void ItShouldHaveMergedOverlappingSlots()
            => Assert.That(_tc?.Slots, Is.EqualTo(new[]
                {
                        (_activityIdTwo: _tc?.ActivityIdTwo, 0, 96)
                }));
        [Test]
        public void ItShouldSummarize()
        {
            var summary = _tc!.Timesheet.Summarize()
                .Select(p =>
                {
                    return (p.ProjectId, p.Duration,
                        p.Activities.Select(a => (a.ActivityId, a.Duration)).ToArray());
                });

            Assert.That(summary, Is.EqualTo(new[]
            {
                    (_projectId: _tc.ProjectId, 96, new []
                    {
                        (_activityIdTwo: _tc.ActivityIdTwo, 96),
                    })
                }));
        }
    }

    public class WhenErasingAnEmptyTimesheet
    {
        private TestContext _tc;

        [OneTimeSetUp]
        public void Setup()
        {
            _tc = new TestContext();
            _tc?.Timesheet.Register(new ActivityTimeSlot(_tc.ProjectId, _tc.ActivityIdOne, 8, 2));
            _tc?.Timesheet.Register(new ActivityTimeSlot(_tc.ProjectId, _tc.ActivityIdOne, 10, 8));
            _tc?.Timesheet.Register(new EraseTimeSlot(0, 96));
        }

        [Test]
        public void ItShouldNotRegisterAnyTime()
            => Assert.That(_tc?.Timesheet.TotalMinutes(), Is.Zero);

        [Test]
        public void ItShouldHaveZeroSlots()
            => Assert.That(_tc?.Slots, Is.Empty);

        [Test]
        public void ItShouldSummarize()
        {
            var summary = _tc!.Timesheet.Summarize()
                .Select(p =>
                {
                    return (p.ProjectId, p.Duration,
                        p.Activities.Select(a => (a.ActivityId, a.Duration)).ToArray());
                });

            Assert.That(summary, Is.Empty);
        }
    }

    public class WhenErasingASingleActivity
    {
        private TestContext _tc;

        [OneTimeSetUp]
        public void Setup()
        {
            _tc = new TestContext();
            _tc?.Timesheet.Register(new ActivityTimeSlot(_tc.ProjectId, _tc.ActivityIdOne, 0, 3));
            _tc?.Timesheet.Register(new ActivityTimeSlot(_tc.ProjectId, _tc.ActivityIdOne, 10, 3));
            _tc?.Timesheet.Register(new EraseTimeSlot(10, 3));
        }

        [Test]
        public void ItShouldBeReflectedInTotalMinutes()
            => Assert.That(_tc?.Timesheet.TotalMinutes(), Is.EqualTo(3 * 15));

        [Test]
        public void ItShouldHaveRemovedOneSlot()
            => Assert.That(_tc?.Slots, Is.EqualTo(new[]
            {
                    (_activityIdOne: _tc?.ActivityIdOne, 0, 3),
            }));

        [Test]
        public void ItShouldSummarize()
        {
            var summary = _tc!.Timesheet.Summarize()
                .Select(p =>
                {
                    return (p.ProjectId, p.Duration,
                        p.Activities.Select(a => (a.ActivityId, a.Duration)).ToArray());
                });

            Assert.That(summary, Is.EqualTo(new[]
            {
                    (_projectId: _tc.ProjectId, 3, new []
                    {
                        (_activityIdOne: _tc.ActivityIdOne, 3),
                    })
                }));
        }
    }

    public class WhenErasingHeadPartFromSlot
    {
        private TestContext _tc;

        [OneTimeSetUp]
        public void Setup()
        {
            _tc = new TestContext();
            _tc?.Timesheet.Register(new ActivityTimeSlot(_tc.ProjectId, _tc.ActivityIdOne, 0, 8));
            _tc?.Timesheet.Register(new EraseTimeSlot(0, 4));
        }

        [Test]
        public void ItShouldBeReflectedInTotalMinutes()
            => Assert.That(_tc?.Timesheet.TotalMinutes(), Is.EqualTo(4 * 15));

        [Test]
        public void ItShouldHaveShrunkOverlappedSlot()
            => Assert.That(_tc?.Slots, Is.EqualTo(new[]
            {
                    (_activityIdOne: _tc?.ActivityIdOne, 4, 4),
            }));

        [Test]
        public void ItShouldSummarize()
        {
            var summary = _tc!.Timesheet.Summarize()
                .Select(p =>
                {
                    return (p.ProjectId, p.Duration,
                        p.Activities.Select(a => (a.ActivityId, a.Duration)).ToArray());
                });

            Assert.That(summary, Is.EqualTo(new[]
            {
                    (_projectId: _tc.ProjectId, 4, new []
                    {
                        (_activityIdOne: _tc.ActivityIdOne, 4),
                    })
                }));
        }
    }

    public class WhenErasingHeadPartFromSlotUpwards
    {
        private TestContext _tc;

        [OneTimeSetUp]
        public void Setup()
        {
            _tc = new TestContext();
            _tc?.Timesheet.Register(new ActivityTimeSlot(_tc.ProjectId, _tc.ActivityIdOne, 6, 8));
            _tc?.Timesheet.Register(new EraseTimeSlot(3, 4));
        }

        [Test]
        public void ItShouldBeReflectedInTotalMinutes()
            => Assert.That(_tc?.Timesheet.TotalMinutes(), Is.EqualTo(7 * 15));

        [Test]
        public void ItShouldHaveShrunkOverlappedSlot()
            => Assert.That(_tc?.Slots, Is.EqualTo(new[]
            {
                    (_activityIdOne: _tc?.ActivityIdOne, 7, 7),
            }));

        [Test]
        public void ItShouldSummarize()
        {
            var summary = _tc!.Timesheet.Summarize()
                .Select(p =>
                {
                    return (p.ProjectId, p.Duration,
                        p.Activities.Select(a => (a.ActivityId, a.Duration)).ToArray());
                });

            Assert.That(summary, Is.EqualTo(new[]
            {
                    (_projectId: _tc.ProjectId, 7, new []
                    {
                        (_activityIdOne: _tc.ActivityIdOne, 7),
                    })
                }));
        }
    }

    public class WhenErasingTailPartFromSlot
    {
        private TestContext _tc;

        [OneTimeSetUp]
        public void Setup()
        {
            _tc = new TestContext();
            _tc?.Timesheet.Register(new ActivityTimeSlot(_tc.ProjectId, _tc.ActivityIdOne, 0, 8));
            _tc?.Timesheet.Register(new EraseTimeSlot(4, 4));
        }

        [Test]
        public void ItShouldBeReflectedInTotalMinutes()
            => Assert.That(_tc?.Timesheet.TotalMinutes(), Is.EqualTo(4 * 15));

        [Test]
        public void ItShouldHaveShrunkOverlappedSlot()
            => Assert.That(_tc?.Slots, Is.EqualTo(new[]
            {
                    (_activityIdOne: _tc?.ActivityIdOne, 0, 4),
            }));

        [Test]
        public void ItShouldSummarize()
        {
            var summary = _tc!.Timesheet.Summarize()
                .Select(p =>
                {
                    return (p.ProjectId, p.Duration,
                        p.Activities.Select(a => (a.ActivityId, a.Duration)).ToArray());
                });

            Assert.That(summary, Is.EqualTo(new[]
            {
                    (_projectId: _tc.ProjectId, 4, new []
                    {
                        (_activityIdOne: _tc.ActivityIdOne, 4),
                    })
                }));
        }
    }

    public class WhenErasingASubsetSlot
    {
        private TestContext _tc;

        [OneTimeSetUp]
        public void Setup()
        {
            _tc = new TestContext();
            _tc?.Timesheet.Register(new ActivityTimeSlot(_tc.ProjectId, _tc.ActivityIdOne, 6 * 4, 13 * 4));
            _tc?.Timesheet.Register(new EraseTimeSlot(12 * 4, 4));
        }

        [Test]
        public void ItShouldBeReflectedInTotalMinutes()
            => Assert.That(_tc?.Timesheet.TotalMinutes(), Is.EqualTo(6 * 4 * 15 + 6 * 4 * 15));

        [Test]
        public void ItShouldHaveSplitOverlappedSlot()
            => Assert.That(_tc?.Slots, Is.EqualTo(new[]
            {
                    (_activityIdOne: _tc?.ActivityIdOne, 6*4, 6*4),
                    (_activityIdOne: _tc?.ActivityIdOne, 13*4, 6*4),
            }));

        [Test]
        public void ItShouldSummarize()
        {
            var summary = _tc!.Timesheet.Summarize()
                .Select(p =>
                {
                    return (p.ProjectId, p.Duration,
                        p.Activities.Select(a => (a.ActivityId, a.Duration)).ToArray());
                });

            Assert.That(summary, Is.EqualTo(new[]
            {
                    (_projectId: _tc.ProjectId, 12*4, new []
                    {
                        (_activityIdOne: _tc.ActivityIdOne, 12*4),
                    })
                }));
        }

        [Test]
        public void ItShouldReturnTheFirstHourUsed()
            => Assert.That(_tc?.Timesheet.FirstHourInUse, Is.EqualTo(6));

        [Test]
        public void ItShouldReturnTheLastHourUsed()
            => Assert.That(_tc?.Timesheet.LastHourInUse, Is.EqualTo(19));
    }

    public class WhenErasingHeadAndTail
    {
        private TestContext _tc;

        [OneTimeSetUp]
        public void Setup()
        {
            _tc = new TestContext();
            _tc?.Timesheet.Register(new ActivityTimeSlot(_tc.ProjectId, _tc.ActivityIdOne, 0, 96));
            _tc?.Timesheet.Register(new EraseTimeSlot(0, 12));
            _tc?.Timesheet.Register(new EraseTimeSlot(84, 12));
        }

        [Test]
        public void ItShouldBeReflectedInTotalMinutes()
            => Assert.That(_tc?.Timesheet.TotalMinutes(), Is.EqualTo(18 * 4 * 15));

        [Test]
        public void ItShouldHaveSingleSlot()
            => Assert.That(_tc?.Slots, Is.EqualTo(new[]
            {
                    (_activityIdOne: _tc?.ActivityIdOne, 12, 18 * 4),
            }));

        [Test]
        public void ItShouldReturnTheFirstHourUsed()
            => Assert.That(_tc?.Timesheet.FirstHourInUse, Is.EqualTo(3));

        [Test]
        public void ItShouldReturnTheLastHourUsed()
            => Assert.That(_tc?.Timesheet.LastHourInUse, Is.EqualTo(21));
    }

    public class WhenRegisteringMultipleProjects
    {
        private TestContext _tc;

        [OneTimeSetUp]
        public void Setup()
        {
            _tc = new TestContext();
            _tc?.Timesheet.Register(new ActivityTimeSlot(_tc.ProjectId, _tc.ActivityIdOne, 0, 3));
            _tc?.Timesheet.Register(new ActivityTimeSlot(_tc.ProjectIdTwo, _tc.ActivityIdTwo, 10, 3));
        }

        [Test]
        public void ItShouldBeReflectedInTotalMinutes()
            => Assert.That(_tc?.Timesheet.TotalMinutes(), Is.EqualTo(90));

        [Test]
        public void ItShouldHaveRegisteredSlots()
            => Assert.That(_tc?.Slots, Is.EqualTo(new[]
            {
                    (_tc?.ActivityIdOne, 0, 3),
                    (_tc?.ActivityIdTwo, 10, 3)
            }));

        [Test]
        public void ItShouldSummarize()
        {
            var summary = _tc!.Timesheet.Summarize()
                .Select(p =>
                {
                    return (p.ProjectId, p.Duration,
                        p.Activities.Select(a => (a.ActivityId, a.Duration)).ToArray());
                });

            Assert.That(summary, Is.EqualTo(new[]
            {
                    (_tc.ProjectId, 3, new []
                    {
                        (_activityIdOne: _tc.ActivityIdOne, 3)
                    }),
                    (_tc.ProjectIdTwo, 3, new []
                    {
                        (_activityIdTwo: _tc.ActivityIdTwo, 3)
                    })
                }));
        }
    }

    public class WhenErasingAllPreviousActivities
    {
        private TestContext _tc;

        [OneTimeSetUp]
        public void Setup()
        {
            _tc = new TestContext();
            _tc.Timesheet.Register(new ActivityTimeSlot(_tc.ProjectId, _tc.ActivityIdOne, 0, 3));
            _tc.Timesheet.Register(new ActivityTimeSlot(_tc.ProjectIdTwo, _tc.ActivityIdTwo, 94, 2));
            _tc.Timesheet.Register(new EraseTimeSlot(0, 96));
        }

        [Test]
        public void ItShouldReturnZeroTotalMinutes()
            => Assert.That(_tc?.Timesheet.TotalMinutes(), Is.Zero);

        [Test]
        public void ItShouldHaveAnZeroSlots()
            => Assert.That(_tc?.Timesheet.Slots(), Is.Empty);

        [Test]
        public void ItShouldHaveAnEmptySummary()
            => Assert.That(_tc?.Timesheet.Summarize(), Is.Empty);

        [Test]
        public void ItShouldNotHaveAFirstHourUsed()
            => Assert.That(_tc?.Timesheet.FirstHourInUse, Is.Null);

        [Test]
        public void ItShouldNotHaveALastHourUsed()
            => Assert.That(_tc?.Timesheet.LastHourInUse, Is.Null);
    }

    private class TestContext
    {
        public readonly Timesheet Timesheet;
        public readonly IdOf<Project> ProjectId;
        public readonly IdOf<Project> ProjectIdTwo;
        public readonly IdOf<Activity> ActivityIdOne;
        public readonly IdOf<Activity> ActivityIdTwo;

        public TestContext()
        {
            ProjectId = IdOf<Project>.Random();
            ProjectIdTwo = IdOf<Project>.Random();
            ActivityIdOne = IdOf<Activity>.Random();
            ActivityIdTwo = IdOf<Activity>.Random();
            var date = new Date(DateTime.Parse("2021-08-09T00:00:00Z"));
            Timesheet = Timesheet.CreateForDate(date);
        }

        public IEnumerable<(IdOf<Activity> ActivityId, int Offset, int Duration)> Slots
            => Timesheet.Slots().Select(s => (s.ActivityId, s.Offset, s.Duration));
    }

    public class ActivityTimeSlotTest
    {
        [Test]
        public void ItShouldThrowIfOffsetIsLessThanZero()
        {
            var ex = Assert.Catch<ArgumentException>(() => new ActivityTimeSlot(IdOf<Project>.Random(), IdOf<Activity>.Random(), -1, 1));
            Assert.That(ex?.Message, Does.Contain("Offset cannot be negative. Was -1"));
        }

        [Test]
        public void ItShouldThrowIfOffsetIsGreaterThan96()
        {
            var ex = Assert.Catch<ArgumentException>(() => new ActivityTimeSlot(IdOf<Project>.Random(), IdOf<Activity>.Random(), 97, 1));
            Assert.That(ex?.Message, Does.Contain("Offset must be less than 96. Was 97"));
        }

        [Test]
        public void ItShouldThrowIfOffsetAndDurationIsGreaterThan96()
        {
            var ex = Assert.Catch<ArgumentException>(() => new ActivityTimeSlot(IdOf<Project>.Random(), IdOf<Activity>.Random(), 1, 96));
            Assert.That(ex?.Message, Does.Contain("Range exceeds max quarters per day with 1"));
        }

        [Test]
        public void ItShouldThrowIfDurationIsLessThanOne()
        {
            var ex = Assert.Catch<ArgumentException>(() => new ActivityTimeSlot(IdOf<Project>.Random(), IdOf<Activity>.Random(), 1, 0));
            Assert.That(ex?.Message, Does.Contain("Duration must be greater than zero. Was 0"));
        }

        [Test]
        public void ItShouldBeEqual()
        {
            var projectId = IdOf<Project>.Random();
            var activityId = IdOf<Activity>.Random();
            var a = new ActivityTimeSlot(projectId, activityId, 1, 1);
            var b = new ActivityTimeSlot(projectId, activityId, 1, 1);

            Assert.That(a, Is.EqualTo(b));
        }

        public static IEnumerable<object[]> NotEqualTests()
        {
            var projectId = IdOf<Project>.Random();
            var activityId = IdOf<Activity>.Random();

            yield return new object[]
            {
                    new ActivityTimeSlot(projectId, activityId, 0, 1),
                    new ActivityTimeSlot(projectId, activityId, 1, 1)
            };
            yield return new object[]
            {
                    new ActivityTimeSlot(projectId, activityId, 0, 1),
                    new ActivityTimeSlot(projectId, activityId, 0, 2)
            };
            yield return new object[]
            {
                    new ActivityTimeSlot(IdOf<Project>.Random(), activityId, 0, 1),
                    new ActivityTimeSlot(IdOf<Project>.Random(), activityId, 0, 1)
            };
            yield return new object[]
            {
                    new ActivityTimeSlot(projectId, IdOf<Activity>.Random(), 0, 1),
                    new ActivityTimeSlot(projectId, IdOf<Activity>.Random(), 0, 1)
            };
        }

        [TestCaseSource(nameof(NotEqualTests))]
        public void ItShouldNotBeEqual(ActivityTimeSlot a, ActivityTimeSlot b)
            => Assert.That(a, Is.Not.EqualTo(b));
    }
}
