using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Quarter.Core.Options;
using Quarter.Core.Repositories;

namespace Quarter.Services;

public record struct SystemMetrics(int NumberOfUsers);

public interface IAdminService
{
    bool IsUserRegistrationOpen();

    /// <summary>
    /// Get system metrics that is displayed in the admin view.
    /// </summary>
    /// <param name="ct">Token used to cancel the operation</param>
    /// <returns>System metrics</returns>
    Task<SystemMetrics> GetSystemMetricsAsync(CancellationToken ct);
}

public class AdminService(
    IOptions<AuthOptions> options,
    IRepositoryFactory repositoryFactory)
    : IAdminService
{
    public bool IsUserRegistrationOpen()
        => options?.Value.OpenUserRegistration ?? false;

    public async Task<SystemMetrics> GetSystemMetricsAsync(CancellationToken ct)
    {
        await Task.CompletedTask;
        var numberOfUsers = await repositoryFactory.UserRepository().TotalCountAsync(ct);
        return new SystemMetrics(numberOfUsers);
    }
}
