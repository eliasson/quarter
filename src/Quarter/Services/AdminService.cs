 using Microsoft.Extensions.Options;
using Quarter.Core.Options;

namespace Quarter.Services;

public interface IAdminService
{
    bool IsUserRegistrationOpen();
}

public class AdminService : IAdminService
{
    private readonly IOptions<AuthOptions> _options;

    public AdminService(IOptions<AuthOptions> options)
    {
        _options = options;
    }

    public bool IsUserRegistrationOpen()
        => _options?.Value.OpenRegistration ?? false;
}