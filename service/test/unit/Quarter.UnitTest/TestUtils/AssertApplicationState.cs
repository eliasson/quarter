using System;
using System.Linq;
using NUnit.Framework;
using Quarter.Core.UI.State;
using Quarter.State;

namespace Quarter.UnitTest.TestUtils;

public static class AssertApplicationState
{
    public static void AssertPushedNewModal(this ApplicationState state, Type modalType)
    {
        var modalTypes = state.Modals.Select(m => m.ModalType);
        Assert.That(modalTypes, Is.EqualTo(new[] { modalType }));
    }
}
