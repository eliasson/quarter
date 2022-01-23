using AngleSharp.Dom;
using Bunit;
using Microsoft.AspNetCore.Components;

namespace Quarter.UnitTest.TestUtils;

public class BlazorModalTestCase<T> : BlazorComponentTestCase<T>
    where T : IComponent
{
    protected string Title()
        => ComponentByTestAttribute("modal-title").TextContent;

    protected IElement ConfirmButton()
        => ComponentByTestAttribute("confirm-button");

    protected IElement CancelButton()
        => ComponentByTestAttribute("cancel-button");

    public void Close()
        => ComponentByTestAttribute("modal-close")?.Click();

    protected void Submit()
        => Component?.Find("form").Submit();

    public string ComponentValue(string selector)
        => ComponentByTestAttribute(selector)?.GetAttribute("Value");
}