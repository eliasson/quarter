using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
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

    protected Task<HttpResponseMessage> PostAsync(string requestUri, object payload)
        => HttpTestSession.HttpClient.PostAsync(requestUri, JsonContent.Create(payload));

    protected Task<HttpResponseMessage> DeleteAsync(string? requestUri)
        => HttpTestSession.HttpClient.DeleteAsync(requestUri);

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

    public static async Task<T?> AsPayload<T>(this HttpResponseMessage self)
    {
        var stream = await self.Content.ReadAsStreamAsync();
        stream.Seek(0, SeekOrigin.Begin);
        return JsonSerializer.Deserialize<T>(stream);
    }
}