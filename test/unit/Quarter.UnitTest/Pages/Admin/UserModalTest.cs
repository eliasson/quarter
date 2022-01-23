using AngleSharp.Dom;
using Bunit;
using NUnit.Framework;
using Quarter.Core.Commands;
using Quarter.Pages.Admin.Users;
using Quarter.State;
using Quarter.State.Forms;
using Quarter.UnitTest.TestUtils;

namespace Quarter.UnitTest.Pages.Admin;

public abstract class UserModalTest
{
    public class WhenRenderingNewUserModal : TestCase
    {
        [OneTimeSetUp]
        public void Setup()
        {
            RenderWithParameters(pb =>
                pb.Add(c => c.FormData, new UserFormData()));
        }

        [Test]
        public void ItShouldHaveAConfirmButton()
            => Assert.That(ConfirmButton()?.Text(), Is.EqualTo("Create"));

        [Test]
        public void ItShouldHaveACancelButton()
            => Assert.That(CancelButton()?.Text(), Is.EqualTo("Cancel"));

        [Test]
        public void ItShouldHaveEmailInput()
        {
            var elm = ComponentByTestAttribute("user-email");

            Assert.That(elm?.Attributes["id"]?.Value, Is.EqualTo("user-email"));
        }
    }

    public class WhenSubmittingEmptyForm : TestCase
    {
        [OneTimeSetUp]
        public void Setup()
        {
            RenderWithParameters(pb =>
                pb.Add(c => c.FormData, new UserFormData()));
            Submit();
        }

        [Test]
        public void ItShouldShowValidationErrorForName()
            => Assert.That(EmailFieldValidationMessage().Text() , Is.EqualTo("Email is required"));
    }

    public class WhenSubmittingInvalidEmailField : TestCase
    {
        [OneTimeSetUp]
        public void Setup()
        {
            RenderWithParameters(pb =>
                pb.Add(c => c.FormData, new UserFormData()));
            EmailField().Change("email-address");
            Submit();
        }

        [Test]
        public void ItShouldShowValidationErrorForName()
            => Assert.That(EmailFieldValidationMessage().Text() , Is.EqualTo("The Email field is not a valid e-mail address."));
    }

    [TestFixture]
    public class WhenSubmittingValidForm : TestCase
    {
        [OneTimeSetUp]
        public void Setup()
        {
            RenderWithParameters(pb =>
                pb.Add(c => c.FormData, new UserFormData()));
            EmailField().Change("jane.doe@example.com");
            Submit();
        }

        [Test]
        public void ItShouldDispatchAction()
        {
            var expectedFormData = new UserFormData { Email = "jane.doe@example.com" };
            Assert.True(DidDispatchAction(new AddUserAction(expectedFormData)));
        }
    }

    public class TestCase : BlazorModalTestCase<UserModal>
    {
        protected IElement EmailField()
            => ComponentByTestAttribute("user-email");

        protected IElement EmailFieldValidationMessage()
            => ComponentByTestAttribute("user-email-validation");
    }
}