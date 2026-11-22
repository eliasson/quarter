using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Quarter.Core.Models;
using Quarter.Core.Repositories;
using Quarter.Core.Utils;

namespace Quarter.Services;

/// <summary>
/// Track when users login or when they are being active.
///
/// Since the session uses a sliding expiration window, so a user can be active without logging in again. Therefore, we
/// track activity to update the last login time based on that as well.
/// </summary>
public interface ILoginTracker
{
    /// <summary>
    /// Record a login. Always recorded.
    /// </summary>
    Task RecordLoginInAsync(IdOf<User> userId, CancellationToken ct);

    /// <summary>
    /// Record an activity for an authenticated user. This will update the last login time if the last recorded login
    /// is older than ActivityInterval.
    /// </summary>
    Task RecordActivityAsync(IdOf<User> userId, CancellationToken ct);
}

public class LoginTracker(
    IRepositoryFactory repositoryFactory,
    TimeProvider timeProvider,
    ILogger<LoginTracker> logger)
    : ILoginTracker
{
    public static readonly TimeSpan ActivityInterval = TimeSpan.FromDays(1);

    // An index of the last login/activity per users. This is used to avoid extra database calls for each activity, and
    // since Quarter has such low number of users there is not that much data to keep in memory.
    private readonly ConcurrentDictionary<IdOf<User>, DateTimeOffset> _lastRecorded = new();

    public Task RecordLoginInAsync(IdOf<User> userId, CancellationToken ct)
        => RecordAsync(userId, timeProvider.GetUtcNow(), ct);

    public Task RecordActivityAsync(IdOf<User> userId, CancellationToken ct)
    {
        var now = timeProvider.GetUtcNow();

        // Ignore activity within the allowed interval.
        if (_lastRecorded.TryGetValue(userId, out var lastRecorded) && now - lastRecorded < ActivityInterval)
            return Task.CompletedTask;

        return RecordAsync(userId, now, ct);
    }

    private async Task RecordAsync(IdOf<User> userId, DateTimeOffset now, CancellationToken ct)
    {
        try
        {
            await repositoryFactory.UserRepository().RecordUserLoginAsync(userId, new UtcDateTime(now.UtcDateTime), ct);
            _lastRecorded[userId] = now;
        }
        catch (Exception e) when (e is not OperationCanceledException)
        {
            // Do not cascade exception here, it is not critical. We'll try to update again next time.
            logger.LogWarning(e, "Could not record login for user {UserId}", userId.AsString());
        }
    }
}
