using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Quarter.Core.Exceptions;
using Quarter.Core.Models;
using Quarter.Core.Utils;

namespace Quarter.HttpApi.UnitTest.Services;

[TestFixture]
public class DeleteProjectTest
{
    [TestFixture]
    public class WhenUserHasProjects : TestCase
    {
        private readonly OperationContext _oc = CreateOperationContext();
        private Project? _project;

        [OneTimeSetUp]
        public async Task Setup()
        {
            _project = await AddProject(_oc.UserId, "Project alpha");
            await ApiService.DeleteProjectAsync(_project.Id, _oc, CancellationToken.None);
        }

        [Test]
        public async Task ItShouldHaveDeletedProject()
            => await Assert.ThrowsAsync<NotFoundException>(() => ReadProjectAsync(_oc.UserId, _project!.Id));
    }

    [TestFixture]
    public class WhenNoProjectsExistForUser : TestCase
    {
        private readonly OperationContext _oc = CreateOperationContext();

        [Test]
        public async Task ItShouldBeANoOp()
            => await Assert.DoesNotThrowAsync(() => ApiService.DeleteProjectAsync(IdOf<Project>.Random(), _oc, CancellationToken.None));
    }
}
