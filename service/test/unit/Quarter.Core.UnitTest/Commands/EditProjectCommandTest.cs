using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Quarter.Core.Commands;
using Quarter.Core.Exceptions;
using Quarter.Core.Models;
using Quarter.Core.Utils;

namespace Quarter.Core.UnitTest.Commands;

[TestFixture]
public class EditProjectCommandTest : CommandTestBase
{
    public class WhenProjectDoesNotExist : EditProjectCommandTest
    {
        [Test]
        public void ItShouldFail()
        {
            var command = new EditProjectCommand(IdOf<Project>.Random(), null, null, null);
            Assert.ThrowsAsync<NotFoundException>(() => Handler.ExecuteAsync(command, OperationContext(), CancellationToken.None));
        }
    }

    public class WhenEditingProjectName : EditProjectCommandTest
    {
        public static IEnumerable<object[]> EditTests()
        {
            yield return new object[]
            {
                null, null,
                "Initial name", "Initial description"
            };

            yield return new object[]
            {
                "Updated name", "Updated description",
                "Updated name", "Updated description"
            };
        }

        [TestCaseSource(nameof(EditTests))]
        public async Task ItShouldHaveFieldValue(string name, string description, string expectedName, string expectedDescription)
        {
            var projectRepository = RepositoryFactory.ProjectRepository(ActingUser);
            var initialProject = await projectRepository.CreateAsync(new Project("Initial name", "Initial description", Color.FromHexString("#457b9d")),
                CancellationToken.None);
            var command = new EditProjectCommand(initialProject.Id, name, description, null);
            await Handler.ExecuteAsync(command, OperationContext(), CancellationToken.None);

            var readProject = await projectRepository.GetByIdAsync(initialProject.Id, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(readProject.Name, Is.EqualTo(expectedName));
                Assert.That(readProject.Description, Is.EqualTo(expectedDescription));
            });
        }
    }

    public class WhenEditingProjectColor : EditProjectCommandTest
    {
        [Test]
        public async Task ItShouldUpdateColorWhenProvided()
        {
            var projectRepository = RepositoryFactory.ProjectRepository(ActingUser);
            var initialProject = await projectRepository.CreateAsync(
                new Project("Name", "Desc", Color.FromHexString("#457b9d")), CancellationToken.None);

            var newColor = Color.FromHexString("#e63946");
            var command = new EditProjectCommand(initialProject.Id, null, null, newColor);
            await Handler.ExecuteAsync(command, OperationContext(), CancellationToken.None);

            var readProject = await projectRepository.GetByIdAsync(initialProject.Id, CancellationToken.None);
            Assert.That(readProject.Color, Is.EqualTo(newColor));
        }

        [Test]
        public async Task ItShouldNotChangeColorWhenNull()
        {
            var projectRepository = RepositoryFactory.ProjectRepository(ActingUser);
            var originalColor = Color.FromHexString("#457b9d");
            var initialProject = await projectRepository.CreateAsync(
                new Project("Name", "Desc", originalColor), CancellationToken.None);

            var command = new EditProjectCommand(initialProject.Id, "New name", null, null);
            await Handler.ExecuteAsync(command, OperationContext(), CancellationToken.None);

            var readProject = await projectRepository.GetByIdAsync(initialProject.Id, CancellationToken.None);
            Assert.That(readProject.Color, Is.EqualTo(originalColor));
        }
    }
}
