using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quarter.Core.Auth;
using Quarter.Core.Commands;
using Quarter.Core.Exceptions;
using Quarter.Core.Models;
using Quarter.Core.Options;
using Quarter.Core.Repositories;
using Quarter.Core.Utils;

namespace Quarter.Services;

public enum AuthorizedState
{
    NotAuthorized,
    Authorized,
}

public record AuthorizedResult(AuthorizedState State, List<Claim> Claims)
{
    public static AuthorizedResult Unauthorized()
        => new AuthorizedResult(AuthorizedState.NotAuthorized, []);

    public static AuthorizedResult AuthorizedWith(params Claim[] claims)
        => new AuthorizedResult(AuthorizedState.Authorized, claims.ToList());

    public virtual bool Equals(AuthorizedResult? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return State.Equals(other.State)
               && Claims.SequenceEqual(other.Claims);
    }

    public override int GetHashCode()
        => HashCode.Combine(State, Claims);
}

public interface IUserAuthorizationService
{
    /// <summary>
    /// Authorize any existing users with their given claim.
    ///
    /// If the user does not exist it will either be created (as a standard user), or an unauthorized result
    /// will be returned.
    ///
    /// The above behaviour is determined on whether or not the user registration is open or closed (configurable
    /// in AuthOptions).
    /// </summary>
    /// <param name="email">The potential user email</param>
    /// <param name="ct">The cancellation token</param>
    /// <returns>A result indicating authorization state</returns>
    Task<AuthorizedResult> AuthorizeOrCreateUserAsync(string email, CancellationToken ct);
}

public class UserAuthorizationService(
    IRepositoryFactory repositoryFactory,
    ICommandHandler commandHandler,
    ILoginTracker loginTracker,
    IOptions<AuthOptions> authOptions,
    ILogger<UserAuthorizationService> logger)
    : IUserAuthorizationService
{
    private readonly IUserRepository _userRepository = repositoryFactory.UserRepository();

    public async Task<AuthorizedResult> AuthorizeOrCreateUserAsync(string email, CancellationToken ct)
    {
        try
        {
            return await TryWithExistingUser();
        }
        catch (NotFoundException)
        {
            if (!authOptions.Value.OpenUserRegistration)
            {
                logger.LogInformation("Unauthorized user {Email} tried to login and user registration is closed", email);
                return AuthorizedResult.Unauthorized();
            }

            logger.LogInformation("Unauthorized user {Email} tried to login, creating new user and granting access", email);

            var command = new AddUserCommand(new Email(email), ArraySegment<UserRole>.Empty);

            await commandHandler.ExecuteAsync(command, OperationContext.None, ct);
            return await TryWithExistingUser();
        }

        async Task<AuthorizedResult> TryWithExistingUser()
        {
            var user = await _userRepository.GetUserByEmailAsync(email, ct);
            await loginTracker.RecordLoginInAsync(user.Id, ct);

            logger.LogInformation("Successfully authorized user {Email} at login", email);
            return AuthorizedResult.AuthorizedWith(ClaimsForUser(user).ToArray());
        }
    }

    private static IEnumerable<Claim> ClaimsForUser(User user)
    {
        var claims = new List<Claim>();
        claims.Add(ApplicationClaim.FromUserId(user.Id));

        if (user.IsAdmin())
            claims.Add(new Claim(ClaimTypes.Role, "administrator"));
        return claims;
    }
}
