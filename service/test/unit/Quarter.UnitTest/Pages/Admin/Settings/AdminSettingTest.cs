using NUnit.Framework;
using Quarter.Pages.Admin.Settings;
using Quarter.UnitTest.TestUtils;

namespace Quarter.UnitTest.Pages.Admin.Settings;

[TestFixture]
public class AdminSettingTest
{
    [TestFixture]
    public class WhenSettingIsActive : TestCase
    {
        [OneTimeSetUp]
        public void StartUp()
        {
            RenderWithParameters(b => b
                .Add(p => p.Title, "Some header")
                .Add(p => p.Message, "Some message")
                .Add(p => p.Icon, AppSetting.IconType.Active));
        }

        [Test]
        public void ItShouldUseTheGivenTitle()
            => Assert.That(Title(), Is.EqualTo("Some header"));

        [Test]
        public void ItShouldUseTheGivenMessage()
            => Assert.That(Message(), Is.EqualTo("Some message"));

        [Test]
        public void ItShouldRenderActiveIcon()
            => Assert.That(HasActiveIcon(), Is.True);
    }

    [TestFixture]
    public class WhenSettingIsInActive : TestCase
    {
        [OneTimeSetUp]
        public void StartUp()
        {
            RenderWithParameters(b => b
                .Add(p => p.Title, "Some header")
                .Add(p => p.Message, "Some message")
                .Add(p => p.Icon, AppSetting.IconType.Inactive));
        }

        [Test]
        public void ItShouldRenderInactiveIcon()
            => Assert.That(HasInActiveIcon(), Is.True);
    }

    public abstract class TestCase : BlazorComponentTestCase<AppSetting>
    {
        protected string Title()
            => ComponentByTestAttribute("admin-setting-title").TextContent;

        protected string Message()
            => ComponentByTestAttribute("admin-setting-message").TextContent;

        protected bool HasActiveIcon()
            => ComponentByTestAttribute("setting-icon").InnerHtml.Contains("icon-check-circle");

        protected bool HasInActiveIcon()
            => ComponentByTestAttribute("setting-icon").InnerHtml.Contains("icon-x-circle");
    }
}
