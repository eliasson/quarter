using System.Linq;
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

[TestFixture]
public class ActivityModalTest
{
    public class WhenRenderingEmptyFormData : TestCase
    {
        [OneTimeSetUp]
        public void Setup()
        {
            RenderWithParameters(pb => pb
                .Add(c => c.FormData, new ActivityFormData())
                .Add(c => c.ModalTitle, "Activity modal"));
        }

        [Test]
        public void ItShouldHaveAConfirmButton()
            => Assert.That(ConfirmButton()?.Text(), Is.EqualTo("Create"));

        [Test]
        public void ItShouldHaveACancelButton()
            => Assert.That(CancelButton()?.Text(), Is.EqualTo("Cancel"));

        [Test]
        public void ItShouldHaveExpectedTitle()
            => Assert.That(ComponentByTestAttribute("modal-title").TextContent, Is.EqualTo("Activity modal"));

        [Test]
        public void ItShouldHaveEmptyName()
            => Assert.That(NameField().Value(), Is.Empty);

        [Test]
        public void ItShouldHaveEmptyDescription()
            => Assert.That(DescriptionField().Value(), Is.Empty);

        [Test]
        public void ItShouldHaveRandomColor()
            => Assert.That(ColorField().Value().Length, Is.EqualTo(7));
    }

    public class WhenRenderingWithFromData : TestCase
    {
        [OneTimeSetUp]
        public void Setup()
        {
            RenderWithParameters(pb => pb
                .Add(c => c.FormData, new ActivityFormData
                {
                    Name = "Activity One",
                    Description = "Note",
                    Color = "#123456",
                })
                .Add(c => c.ModalTitle, "Activity modal"));
        }

        [Test]
        public void ItShouldHaveExpectedName()
            => Assert.That(NameField().Value(), Is.EqualTo("Activity One"));

        [Test]
        public void ItShouldHaveExpectedDescription()
            => Assert.That(DescriptionField().Value(), Is.EqualTo("Note"));

        [Test]
        public void ItShouldHaveExpectedColor()
            => Assert.That(ColorField().Attributes["value"]?.Value, Is.EqualTo("#123456"));
    }

    public class WhenSubmittingEmptyForm : TestCase
    {
        [OneTimeSetUp]
        public void Setup()
        {
            RenderWithParameters(pb =>
                pb.Add(c => c.FormData, new ActivityFormData()));
            ColorField().Change("");
            Submit();
        }

        [Test]
        public void ItShouldShowValidationErrorForName()
            => Assert.That(NameFieldValidationMessage().Text() , Is.EqualTo("Name is required"));

        [Test]
        public void ItShouldShowValidationErrorForColor()
            => Assert.That(ColorValidationMessage().Text(), Is.EqualTo("Activity color is required"));
    }

    public class WhenSelectingRandomColorInput : TestCase
    {
        [OneTimeSetUp]
        public void SubmitEmptyForm()
        {
            RenderWithParameters(pb =>
                pb.Add(c => c.FormData, new ActivityFormData()));
            RandomColorButton()?.Click();
        }

        [Test]
        public void ItShouldPopulateColorInputWithColorValue()
        {
            var color = ColorField().Value();
            Assert.That(color, Does.Match("^#([0-9A-F]{6})$"));
        }

        [Test]
        public void ItShouldBeAButtonTypeButton()
            => Assert.That(RandomColorButton().ButtonType(), Is.EqualTo("button"));
    }

    [TestFixture("#112233")]
    [TestFixture("#fff")]
    [TestFixture("#FFF")]
    [TestFixture("#FFF123")]
    [TestFixture("#fff123")]
    public class WhenEnteringValidColorInput : TestCase
    {
        private readonly string _color;

        public WhenEnteringValidColorInput(string color)
        {
            _color = color;
        }

        [OneTimeSetUp]
        public void SubmitEmptyForm()
        {
            RenderWithParameters(pb =>
                pb.Add(c => c.FormData, new ActivityFormData()));
            ColorField()?.Change(_color);
        }

        [Test]
        public void ItShouldUseTheGivenColorAsMarkerBackground()
        {
            var marker = ComponentByTestAttribute("activity-color-marker");
            var inlineStyle = marker.Attributes["style"].Value;
            Assert.That(inlineStyle, Is.EqualTo($"background-color: {_color}"));
        }

        [Test]
        public void ItShouldNotShowValidationErrorForColor()
            => Assert.Catch<ElementNotFoundException>(() => ColorValidationMessage().Text());
    }

    [TestFixture(false)]
    [TestFixture(true)]
    public class WhenSubmittingValidForm : TestCase
    {
        private static readonly IdOf<Project> ProjectId = IdOf<Project>.Random();
        private IdOf<Activity> _activityId;

        public WhenSubmittingValidForm(bool isEditMode)
        {
            if (isEditMode)
                _activityId = IdOf<Activity>.Random();
        }

        [OneTimeSetUp]
        public void Setup()
        {
            RenderWithParameters(pb =>
            {
                pb.Add(c => c.FormData, new ActivityFormData());
                pb.Add(c => c.ProjectId, ProjectId);
                if (_activityId is not null) pb.Add(c => c.ActivityId, _activityId);
            });

            NameField().Change("Activity Alpha");
            DescriptionField().Change("Yup");
            ColorField().Change("#123");
            Submit();
        }

        [Test]
        public void ItShouldDispatchAction()
        {
            var expectedFormData = new ActivityFormData { Name = "Activity Alpha", Description = "Yup", Color = "#123" };
            IAction expectedAction = _activityId is null
                ? new AddActivityAction(ProjectId, expectedFormData)
                : new EditActivityAction(ProjectId, _activityId, expectedFormData);

            Assert.That(DidDispatchAction(expectedAction), Is.True);
        }

        [Test]
        public void ItShouldHaveCorrectConfirmName()
        {
            var expectedName = _activityId is null ? "Create" : "Save";
            Assert.That(ConfirmButton().Text(), Is.EqualTo(expectedName));
        }
    }

    public class TestCase : BlazorModalTestCase<ActivityModal>
    {
        protected IElement NameField()
            => ComponentByTestAttribute("activity-name");

        protected IElement DescriptionField()
            => ComponentByTestAttribute("activity-description");

        protected IElement ColorField()
            => ComponentByTestAttribute("activity-color");

        protected IElement NameFieldValidationMessage()
            => ComponentByTestAttribute("activity-name-validation");

        protected IElement ColorValidationMessage()
            => ComponentByTestAttribute("activity-color-validation");

        protected IElement RandomColorButton()
            => ComponentByTestAttribute("action-button");
    }
}
