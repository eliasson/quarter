using System;
using System.Net.Http;

namespace Quarter.UnitTest.TestUtils;

public static class HttpTestSession
{
    public static HttpClient HttpClient
        => HttpSession.Value.HttpClient;

    private static readonly Lazy<HttpSession> HttpSession = CreateSession();

    private static Lazy<HttpSession> CreateSession()
        => new (() => new HttpSession());
}
