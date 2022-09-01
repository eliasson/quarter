using System.Threading;
using System.Threading.Tasks;
using Quarter.Core.Models;
using Quarter.Core.Repositories;
using Quarter.Core.Utils;
using NUnit.Framework;
using Quarter.Core.Commands;

namespace Quarter.Core.UnitTest.Commands
{
    [TestFixture]
    public class CommandTestBase
    {
        protected InMemoryRepositoryFactory RepositoryFactory;
        protected IUserRepository UserRepository;
        protected ICommandHandler Handler;
        protected readonly IdOf<User> ActingUser = IdOf<User>.Random();

        [OneTimeSetUp]
        protected void SetUpTestBase()
        {
            RepositoryFactory = new InMemoryRepositoryFactory();
            UserRepository = RepositoryFactory.UserRepository();
            Handler = new CommandHandler(RepositoryFactory);
        }

        protected Task<User> AddUserInRepository(Email email, params UserRole[] roles)
            => UserRepository.CreateAsync(new User(email, roles), CancellationToken.None);

        protected OperationContext OperationContext()
            => new OperationContext(ActingUser);

        protected Task<Project> CreateProjectAsync(string name)
        {
            var repo = RepositoryFactory.ProjectRepository(ActingUser);
            return repo.CreateAsync(new Project(name, $"Description for {name}"), CancellationToken.None);
        }

        protected Task<Activity> CreateActivityAsync(IdOf<Project> projectId, string name)
        {
            var repo = RepositoryFactory.ActivityRepository(ActingUser);
            return repo.CreateAsync(new Activity(projectId, name, $"Description for {name}", Color.FromHexString("#123")), CancellationToken.None);
        }

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