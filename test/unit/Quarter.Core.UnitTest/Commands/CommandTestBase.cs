using System.Threading;
using System.Threading.Tasks;
using Quarter.Core.Models;
using Quarter.Core.Repositories;
using Quarter.Core.Utils;
using NUnit.Framework;
using Quarter.Core.Commands;
using Quarter.Core.Events;
using Quarter.Core.UnitTest.TestUtils;

namespace Quarter.Core.UnitTest.Commands
{
    public abstract class CommandTestBase<T> where T : IEvent
    {
        protected InMemoryRepositoryFactory RepositoryFactory;
        protected IUserRepository UserRepository;
        protected ICommandHandler Handler;
        protected TestSubscriber<T> EventSubscriber;
        protected readonly IdOf<User> ActingUser = IdOf<User>.Random();

        [OneTimeSetUp]
        protected void SetUpTestBase()
        {
            RepositoryFactory = new InMemoryRepositoryFactory();
            UserRepository = RepositoryFactory.UserRepository();
            var eventDispatcher = new EventDispatcher();
            EventSubscriber = new TestSubscriber<T>(eventDispatcher);
            Handler = new CommandHandler(RepositoryFactory, eventDispatcher);
        }

        protected Task<User> AddUserInRepository(Email email, params UserRole[] roles)
            => UserRepository.CreateAsync(new User(email, roles), CancellationToken.None);

        protected OperationContext OperationContext()
            => new OperationContext(ActingUser);

        protected async Task RegisterTimeAsync(Date date, Activity activity, int offset, int duration)
        {
            var repo = RepositoryFactory.TimesheetRepository(ActingUser);
            var timesheet = await repo.GetOrNewTimesheetAsync(date, CancellationToken.None);
            await repo.UpdateByIdAsync(timesheet.Id, ts =>
            {
                ts.Register(new ActivityTimeSlot(activity.ProjectId, activity.Id, offset, duration));
                return ts;
            }, CancellationToken.None);
        }

        protected Task<Timesheet> GetTimesheetAsync(Date date)
        {
            var repo = RepositoryFactory.TimesheetRepository(ActingUser);
            return repo.GetOrNewTimesheetAsync(date, CancellationToken.None);
        }
    }
}