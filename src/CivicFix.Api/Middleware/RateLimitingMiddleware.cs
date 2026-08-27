using System.Collections.Concurrent;
using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace CivicFix.Api.Middleware;

public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RateLimitingMiddleware> _logger;
    private static readonly ConcurrentDictionary<string, List<DateTime>> RequestLog = new();
    private const int MaxRequestsPerMinute = 60;
    private static readonly TimeSpan WindowDuration = TimeSpan.FromMinutes(1);

    public RateLimitingMiddleware(RequestDelegate next, ILogger<RateLimitingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Only throttle mutating API endpoints (POST/PUT/DELETE)
        if (context.Request.Method == HttpMethods.Post || 
            context.Request.Method == HttpMethods.Put || 
            context.Request.Method == HttpMethods.Delete)
        {
            var clientKey = context.Connection.RemoteIpAddress?.ToString() ?? "unknown_client";
            var now = DateTime.UtcNow;

            var timestamps = RequestLog.GetOrAdd(clientKey, _ => new List<DateTime>());

            lock (timestamps)
            {
                // Remove expired timestamps
                timestamps.RemoveAll(t => now - t > WindowDuration);

                if (timestamps.Count >= MaxRequestsPerMinute)
                {
                    _logger.LogWarning("Rate limit threshold exceeded for client {ClientKey}", clientKey);
                    context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
                    context.Response.ContentType = "application/problem+json";

                    var problem = new
                    {
                        type = "https://tools.ietf.org/html/rfc7807#section-6.5.15",
                        title = "Rate Limit Exceeded",
                        status = 429,
                        detail = "Too many requests. Please slow down and try again in 60 seconds."
                    };

                    var json = JsonSerializer.Serialize(problem);
                    return;
                }

                timestamps.Add(now);
            }
        }

        await _next(context);
    }
}
