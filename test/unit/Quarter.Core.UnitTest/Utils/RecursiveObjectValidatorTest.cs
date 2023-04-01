using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using NUnit.Framework;
using Quarter.Core.Utils;

namespace Quarter.Core.UnitTest.Utils;

#nullable enable

[TestFixture]
public class RecursiveObjectValidatorTest
{
    [Test]
    public void ItShouldValidateObject()
    {
        var prospect = new Root
        {
            RootValue = 42,
            Child = new Child { ChildValue = 1 },
        };

        var result = RecursiveObjectValidator.IsValid(prospect, out var errors);
        var messages = errors.Select(_ => _.ErrorMessage);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.True);
            Assert.That(messages, Is.Empty);
        });
    }

    [Test]
    public void ItShouldFailValidationWhenRootLevelPropertyIsMissing()
    {
        var prospect = new Root();

        var result = RecursiveObjectValidator.IsValid(prospect, out var errors);
        var messages = errors.Select(_ => _.ErrorMessage);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.False);
            Assert.That(messages, Does.Contain("The RootValue field is required."));
        });
    }

    [Test]
    public void ItShouldFailValidationWhenChildLevelPropertyIsMissing()
    {
        var prospect = new Root {  Child = new Child() };

        var result = RecursiveObjectValidator.IsValid(prospect, out var errors);
        var messages = errors.Select(_ => _.ErrorMessage);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.False);
            Assert.That(messages, Does.Contain("The ChildValue field is required."));
        });
    }

    [Test]
    public void ItShouldValidateWhenChildIsNotInstantiated()
    {
        var prospect = new Root { RootValue = 42 };

        var result = RecursiveObjectValidator.IsValid(prospect, out var errors);
        var messages = errors.Select(_ => _.ErrorMessage);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.True);
            Assert.That(messages, Is.Empty);
        });
    }

    [Test]
    public void ItShouldFailValidationWhenListOchChildLevelPropertyIsMissing()
    {
        var prospect = new Root { Children = new [] { new Child() } };

        var result = RecursiveObjectValidator.IsValid(prospect, out var errors);
        var messages = errors.Select(_ => _.ErrorMessage);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.False);
            Assert.That(messages, Does.Contain("The ChildValue field is required."));
        });
    }

    [Test]
    public void ItWillNotLoopInfinity()
    {
        var prospect = new Root { RootValue = 42 };
        prospect.Child = new Child { ChildValue = 1, Root = prospect };

        var result = RecursiveObjectValidator.IsValid(prospect, out var errors);
        var messages = errors.Select(_ => _.ErrorMessage);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.True);
            Assert.That(messages, Is.Empty);
        });
    }
}

public class Root
{
    [Required]
    public int? RootValue { get; set; }

    public Child? Child { get; set; }

    public IList<Child>? Children { get; set; }
}

public class Child
{
    [Required]
    public int? ChildValue { get; set; }

    public Root? Root { get; set; }
}