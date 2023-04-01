using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Quarter.Core.Models;
using Quarter.Core.Repositories;
using Quarter.Core.UnitTest.TestUtils;
using Quarter.Core.Utils;

namespace Quarter.UnitTest.TestUtils;

#nullable enable

[Category(TestCategories.DatabaseDependency)]
public class HttpTestCase
{
    // This might be expensive, starting a new server for each test. But the way authentication is faked it is not
    // thread safe and tests interfere with each other
    protected readonly HttpSession HttpTestSession = new HttpSession();

    protected Task<User> SetupUnauthenticatedUserAsync(string email)
    {
        var user = new User(new Email(email));
        HttpTestSession.ClearFakeUserClaims();
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

    protected Task<HttpResponseMessage> PutAsync(string requestUri, object payload)
        => HttpTestSession.HttpClient.PutAsync(requestUri, JsonContent.Create(payload));

    protected Task<HttpResponseMessage> PatchAsync(string requestUri, object payload)
        => HttpTestSession.HttpClient.PatchAsync(requestUri, JsonContent.Create(payload));

    protected Task<HttpResponseMessage> DeleteAsync(string? requestUri)
        => HttpTestSession.HttpClient.DeleteAsync(requestUri);

    protected Task<Project> AddProjectAsync(IdOf<User> userId, string name)
    {
        var repoFactory = HttpTestSession.ResolveService<IRepositoryFactory>();
        var project = new Project(name, $"description:{name}");
        return repoFactory.ProjectRepository(userId).CreateAsync(project, CancellationToken.None);
    }

    protected Task<Activity> AddActivityAsync(IdOf<User> userId, IdOf<Project> projectId, string name)
    {
        var repoFactory = HttpTestSession.ResolveService<IRepositoryFactory>();
        var activity = new Activity(projectId, name, $"description:{name}", Color.FromHexString("#FFFFFF"));
        return repoFactory.ActivityRepository(userId).CreateAsync(activity, CancellationToken.None);
    }

    protected Task<Timesheet> AddTimesheetAsync(IdOf<User> userId, Date date, params ActivityTimeSlot[] timeSlots)
    {
        var timesheet = Timesheet.CreateForDate(date);
        foreach (var slot in timeSlots)
            timesheet.Register(slot);

        var repoFactory = HttpTestSession.ResolveService<IRepositoryFactory>();
        return repoFactory.TimesheetRepository(userId).CreateAsync(timesheet, CancellationToken.None);
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