using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Components;

namespace Quarter.UnitTest.TestUtils;

/// <summary>
/// Override the ordinary untestable navigation manager with one that mocks the invoked
/// navigations.
/// </summary>
public class TestNavigationManager : NavigationManager
{
    private readonly string _baseUri = "https://example.com/";
    private readonly IList<string> _performedNavigations = new List<string>();

    public TestNavigationManager()
        => Initialize(_baseUri, _baseUri);

    public string LastNavigatedTo()
        => _performedNavigations.Last();

    protected override void NavigateToCore(string uri, bool forceLoad)
        => _performedNavigations.Add(uri);
}
