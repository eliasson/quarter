using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AngleSharp.Dom;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Quarter.Core.Events;
using Quarter.Core.Repositories;
using Quarter.Core.UI.State;
using Quarter.Services;
using Quarter.State;

namespace Quarter.UnitTest.TestUtils
{
    public abstract class BlazorComponentTestCase<T> where T : IComponent
    {
        protected readonly TestContext Context = new TestContext();
        protected IRenderedComponent<T> Component;
        protected readonly TestStateManager StateManager = new TestStateManager();
        protected readonly IRepositoryFactory RepositoryFactory = new InMemoryRepositoryFactory();
        protected readonly IEventDispatcher EventDispatcher = new EventDispatcher();

        protected BlazorComponentTestCase()
        {
            ConfigureTestContext(Context);
        }

        protected void Render()
            => Component = Context.RenderComponent<T>();

        protected void RenderWithParameters(Action<ComponentParameterCollectionBuilder<T>> parameterBuilder)
            => Component = Context.RenderComponent(parameterBuilder);

        protected virtual void ConfigureTestContext(TestContext ctx)
        {
            // Override this to configure which services to inject
            Context.Services.AddScoped<IStateManager<ApplicationState>>(_ => StateManager);
            Context.Services.AddSingleton(RepositoryFactory);
            Context.Services.AddSingleton(EventDispatcher);
            Context.Services.AddScoped<IUserAuthorizationService, TestIUserAuthorizationService>();
        }

        protected string TextForElement(string cssSelector)
            => Component?.Find(cssSelector).TextContent;

        protected IElement ComponentByTestAttribute(string value)
            => Component?.Find($"[test={value}]");

        protected IRefreshableElementCollection<IElement> ComponentsByTestAttribute(string value)
            => Component?.FindAll($"[test={value}]");

        protected bool DidDispatchAction(Type actionType)
        {
            var last = StateManager.DispatchedActions.Last();
            if (last is not null)
                return last.GetType() == actionType;
            return false;
        }

        protected bool DidDispatchAction(IAction action)
            => StateManager.DispatchedActions.FirstOrDefault(a => a.Equals(action)) is not null;

        protected async Task<bool> EventuallyDispatchedAction(IAction action)
        {
            var cts = new CancellationTokenSource();
            var dispatchTask = DoEventually(cts.Token);
            var r = await Task.WhenAny(dispatchTask, Task.Delay(3000, cts.Token));
            if (r.IsCompleted && r is Task<bool> bt)
                return bt.Result;
            return false;

            async Task<bool> DoEventually(CancellationToken ct)
            {
                while (!DidDispatchAction(action))
                {
                    await Task.Delay(10, ct);
                }

                return true;
            }
        }
    }
}