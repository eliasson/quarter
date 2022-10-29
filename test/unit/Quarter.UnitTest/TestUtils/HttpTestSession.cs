using System;
using System.Net.Http;
using Quarter.Core.Models;

namespace Quarter.UnitTest.TestUtils;

public static class HttpTestSession
{
    public static HttpClient HttpClient
        => HttpSession.Value.HttpClient;

    public static T ResolveService<T>() where T : class
        => HttpSession.Value.ResolveService<T>();

    public static void LogoutFakeUser()
        => HttpSession.Value.ClearFakeUserClaims();

    public static void FakeUserSession(User user)
        => HttpSession.Value.FakeUserSession(user);

    private static readonly Lazy<HttpSession> HttpSession = CreateSession();

    private static Lazy<HttpSession> CreateSession()
        => new (() => new HttpSession());
}
