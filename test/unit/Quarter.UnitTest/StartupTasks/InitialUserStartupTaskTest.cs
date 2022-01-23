using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using Quarter.Core.Commands;
using Quarter.Core.Models;
using Quarter.Core.Options;
using Quarter.Core.Repositories;
using Quarter.Core.Utils;
using Quarter.StartupTasks;
using Quarter.UnitTest.TestUtils;

namespace Quarter.UnitTest.StartupTasks;

public abstract class InitialUserStartupTaskTest
{
    public class WhenNotConfigured : TestCase
    {
        [OneTimeSetUp]
        public async Task Setup()
        {
            InitializeNoConfig();
            await StartupTask.ExecuteAsync();
        }

        [Test]
        public void ItShouldNotCreateAnyUser()
            => Assert.That(CommandHandler.ExecutedCommands, Is.Empty);
    }

    public class WhenConfiguredAndUserDoesNotExist : TestCase
    {
        [OneTimeSetUp]
        public async Task Setup()
        {
            Initialize(new InitialUserOptions { Enabled = true, Email = "initial@example.com" });
            await StartupTask.ExecuteAsync();
        }

        [Test]
        public void ItShouldCreateUser()
        {
            var cmd = CommandHandler.LastExecutedCommandOrFail<AddUserCommand>();
            Assert.Multiple(() =>
            {
                Assert.That(cmd.Email.Value, Is.EqualTo("initial@example.com"));
                Assert.That(cmd.Roles, Is.EqualTo(new [] { UserRole.Administrator }));
            });
        }
    }

    public class WhenConfiguredAndUserAlreadyExist : TestCase
    {
        [OneTimeSetUp]
        public async Task Setup()
        {
            Initialize(new InitialUserOptions { Enabled = true, Email = "existing@example.com" });
            await AddUserAsync("existing@example.com");
            await StartupTask.ExecuteAsync();
        }

        [Test]
        public void ItShouldNotCreateAnyUser()
            => Assert.That(CommandHandler.ExecutedCommands, Is.Empty);
    }

    public class WhenConfiguredButDisabled : TestCase
    {
        [OneTimeSetUp]
        public async Task Setup()
        {
            Initialize(new InitialUserOptions { Enabled = false, Email = "initial@example.com" });
            await StartupTask.ExecuteAsync();
        }

        [Test]
        public void ItShouldNotCreateAnyUser()
            => Assert.That(CommandHandler.ExecutedCommands, Is.Empty);
    }

    public class TestCase
    {
        protected readonly TestCommandHandler CommandHandler = new();
        protected InitialUserStartupTask StartupTask;
        private IUserRepository _userRepository;

        protected void InitializeNoConfig()
        {
            Initialize(null);
        }

        protected void Initialize(InitialUserOptions options)
        {
            var repositoryFactory = new InMemoryRepositoryFactory();
            _userRepository = repositoryFactory.UserRepository();
            StartupTask = new InitialUserStartupTask(
                NullLogger<InitialUserStartupTask>.Instance,
                Microsoft.Extensions.Options.Options.Create(options),
                repositoryFactory,
                CommandHandler);
        }

        protected Task AddUserAsync(string email)
        {
            var user = User.AdminUser(new Email(email));
            return _userRepository.CreateAsync(user, CancellationToken.None);
        }
    }
}
