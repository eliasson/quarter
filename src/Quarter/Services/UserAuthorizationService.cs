using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;
using Quarter.Auth;
using Quarter.Core.Auth;
using Quarter.Core.Exceptions;
using Quarter.Core.Models;
using Quarter.Core.Repositories;

namespace Quarter.Services
{
    public enum AuthorizedState
    {
        NotAuthorized,
        Authorized,
    }

    public record AuthorizedResult(AuthorizedState State, List<Claim> Claims)
    {
        public static AuthorizedResult Unauthorized()
            => new AuthorizedResult(AuthorizedState.NotAuthorized, new List<Claim>());

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
        /// Check if there is a user existing with the given email address. For non-existing
        /// users this will return an unauthorized result.
        /// </summary>
        /// <param name="email">The potential user email</param>
        /// <param name="ct">The cancellation token</param>
        /// <returns>A result indicating authorization state</returns>
        Task<AuthorizedResult> IsUserAuthorized(string email, CancellationToken ct);

        /// <summary>
        /// Get the currently logged in user ID if there is a known user for the current session.
        /// </summary>
        /// <returns>The User ID or null</returns>
        Task<IdOf<User>> CurrentUserId();
    }

    public class UserAuthorizationService : IUserAuthorizationService
    {
        private readonly IUserRepository _userRepository;
        private readonly AuthenticationStateProvider _authenticationStateProvider;
        private readonly ILogger<UserAuthorizationService> _logger;

        public UserAuthorizationService(
            AuthenticationStateProvider authenticationStateProvider,
            IRepositoryFactory repositoryFactory,
            ILogger<UserAuthorizationService> logger)
        {
            _userRepository = repositoryFactory.UserRepository();
            _authenticationStateProvider = authenticationStateProvider;
            _logger = logger;
        }

        public async Task<AuthorizedResult> IsUserAuthorized(string email, CancellationToken ct)
        {
            try
            {
                var user = await _userRepository.GetUserByEmailAsync(email, ct);
                _logger.LogInformation("Successfully authorized user {Email} at login", email);
                return AuthorizedResult.AuthorizedWith(ClaimsForUser(user).ToArray());
            }
            catch (NotFoundException)
            {
                _logger.LogInformation("Unauthorized user {Email} tried to login", email);
                return AuthorizedResult.Unauthorized();
            }
        }

        public async Task<IdOf<User>> CurrentUserId()
        {
            var state = await _authenticationStateProvider.GetAuthenticationStateAsync();
            var userId = state.User.Claims
                .FirstOrDefault(c => c.Type == ApplicationClaim.QuarterUserIdClaimType);

            return userId == null
                ? throw new UnauthorizedAccessException("No user found on session!")
                : IdOf<User>.Of(userId.Value);
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
}