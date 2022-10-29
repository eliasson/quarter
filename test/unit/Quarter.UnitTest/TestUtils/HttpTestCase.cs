using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Quarter.Core.Models;
using Quarter.Core.Repositories;
using Quarter.Core.Utils;

namespace Quarter.UnitTest.TestUtils;

#nullable enable

public class HttpTestCase
{
    protected Task<User> SetupUnauthenticatedUserAsync(string email)
    {
        var user = new User(new Email(email));
        HttpTestSession.LogoutFakeUser();
        return Task.FromResult(user);
    }

    protected Task<User> SetupAuthorizedUserAsync(string email)
    {
        var user = new User(new Email(email));
        // TODO: Store user
        HttpTestSession.FakeUserSession(user);
        return Task.FromResult(user);
    }

    protected Task<HttpResponseMessage> GetAsync(string? requestUri)
        => HttpTestSession.HttpClient.GetAsync(requestUri);

    protected Task<Project> AddProjectAsync(IdOf<User> userId, string name)
    {
        var repoFactory = HttpTestSession.ResolveService<IRepositoryFactory>();
        var project = new Project(name, $"description:{name}");
        return repoFactory.ProjectRepository(userId).CreateAsync(project, CancellationToken.None);
    }
}

public static class HttpResponseMessageExtensions
{
    public static string? ContentType(this HttpResponseMessage self)
        => self.Content.Headers.ContentType?.MediaType;
}