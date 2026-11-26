using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace BE.Api.Auth;

public static class RateLimits
{
    public const string Login = nameof(Login);

    /// <summary>Slows down password guessing: max 5 login attempts per minute per IP address.</summary>
    public static void AddHolidayPlannerRateLimits(this RateLimiterOptions options)
    {
        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        options.AddPolicy(Login, context => RateLimitPartition.GetFixedWindowLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions { PermitLimit = 5, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 }));
    }
}
