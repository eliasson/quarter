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
    }
}