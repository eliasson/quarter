using AngleSharp.Dom;
using Bunit;
using NUnit.Framework;
using Quarter.Core.Models;
using Quarter.Core.UI.State;
using Quarter.Pages.Application.Manage;
using Quarter.State;
using Quarter.State.Forms;
using Quarter.UnitTest.TestUtils;

namespace Quarter.UnitTest.Pages.Application.Manage;

public abstract class ProjectModalTest
{
    public class WhenRenderingEmptyFormData : TestCase
    {
        [OneTimeSetUp]
        public void Setup()
        {
            RenderWithParameters(pb => pb
                .Add(c => c.FormData, new ProjectFormData())
                .Add(c => c.ModalTitle, "Project modal"));
        }

        [Test]
        public void ItShouldHaveAConfirmButton()
            => Assert.That(ConfirmButton()?.Text(), Is.EqualTo("Create"));

        [Test]
        public void ItShouldHaveACancelButton()
            => Assert.That(CancelButton()?.Text(), Is.EqualTo("Cancel"));

        [Test]
        public void ItShouldHaveExpectedTitle()
            => Assert.That(ComponentByTestAttribute("modal-title").TextContent, Is.EqualTo("Project modal"));

        [Test]
        public void ItShouldHaveEmptyName()
            => Assert.That(NameField().Value(), Is.Empty);

        [Test]
        public void ItShouldHaveEmptyDescription()
            => Assert.That(DescriptionField().Value(), Is.Empty);
    }

    public class WhenSubmittingEmptyForm : TestCase
    {
        [OneTimeSetUp]
        public void Setup()
        {
            RenderWithParameters(pb =>
                pb.Add(c => c.FormData, new ProjectFormData()));
            Submit();
        }

        [Test]
        public void ItShouldShowValidationErrorForName()
            => Assert.That(NameFieldValidationMessage().Text() , Is.EqualTo("Name is required"));
    }

    [TestFixture(false)]
    [TestFixture(true)]
    public class WhenSubmittingValidForm : TestCase
    {
        private readonly IdOf<Project> _projectId;

        public WhenSubmittingValidForm(bool isEditMode)
        {
            if (isEditMode)
                _projectId = IdOf<Project>.Random();
        }

        [OneTimeSetUp]
        public void Setup()
        {
            RenderWithParameters(pb =>
            {
                pb.Add(c => c.FormData, new ProjectFormData());
                if (_projectId is not null) pb.Add(c => c.ProjectId, _projectId);
            });

            NameField().Change("Doing dishes");
            DescriptionField().Change("The boring stuff");
            Submit();
        }

        [Test]
        public void ItShouldDispatchAction()
        {
            var expectedFormData = new ProjectFormData { Name = "Doing dishes", Description = "The boring stuff" };
            IAction expectedAction = _projectId is null
                ? new AddProjectAction(expectedFormData)
                : new EditProjectAction(_projectId, expectedFormData);

            Assert.True(DidDispatchAction(expectedAction));
        }

        [Test]
        public void ItShouldHaveCorrectConfirmName()
        {
            var expectedName = _projectId is null ? "Create" : "Save";
            Assert.That(ConfirmButton().Text(), Is.EqualTo(expectedName));
        }
    }

    public class TestCase : BlazorModalTestCase<ProjectModal>
    {
        protected IElement NameField()
            => ComponentByTestAttribute("project-name");

        protected IElement DescriptionField()
            => ComponentByTestAttribute("project-description");

        protected IElement NameFieldValidationMessage()
            => ComponentByTestAttribute("project-name-validation");
    }
}