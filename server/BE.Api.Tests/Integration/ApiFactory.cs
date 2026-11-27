using System.Net.Http.Headers;
using BE.Api.Auth;
using BE.Infrastructure.Entities;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace BE.Api.Tests.Integration;

/// <summary>
/// Starts the real API in memory against a real PostgreSQL in a throw-away Docker container
/// (Testcontainers). Migrations run on startup exactly as in production. Requires Docker.
/// </summary>
public sealed class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public const string OwnerEmail = "owner@businessesbjerg.com";
    public const string OwnerPassword = "integration-test-password";

    private readonly PostgreSqlContainer _db = new PostgreSqlBuilder("postgres:16").Build();

    public async Task InitializeAsync()
    {
        await _db.StartAsync();

        // Program.cs reads configuration while building the app, so the values are given as
        // environment variables before the app starts (they win over appsettings and user-secrets).
        Environment.SetEnvironmentVariable("ConnectionStrings__Db", _db.GetConnectionString());
        Environment.SetEnvironmentVariable("Jwt__Key", "integration-test-signing-key-with-enough-length-0123456789");
        Environment.SetEnvironmentVariable("Bootstrap__SystemOwnerEmail", OwnerEmail);
        Environment.SetEnvironmentVariable("Bootstrap__SystemOwnerPassword", OwnerPassword);
    }

    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();
        await _db.DisposeAsync();
    }

    /// <summary>A client without a token.</summary>
    public HttpClient Anonymous() => CreateClient();

    /// <summary>
    /// A client with a valid JWT for the given role. The token is made by the app's own TokenService,
    /// so tests don't hit the login rate limit. (Login itself is tested separately in AuthApiTests.)
    /// </summary>
    public HttpClient As(string role, Guid? employeeId = null)
    {
        var tokens = Services.GetRequiredService<TokenService>();
        var user = new AppUserEntity
        {
            Id = Guid.NewGuid(),
            Email = $"{role.ToLowerInvariant()}-{Guid.NewGuid():N}@businessesbjerg.com",
            Role = role,
            EmployeeId = employeeId,
        };

        var client = CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokens.CreateToken(user).Token);
        return client;
    }
}

/// <summary>All integration test classes share one API + database (starting Docker per class would be slow).</summary>
[CollectionDefinition(Name)]
public class ApiCollection : ICollectionFixture<ApiFactory>
{
    public const string Name = "api";
}
