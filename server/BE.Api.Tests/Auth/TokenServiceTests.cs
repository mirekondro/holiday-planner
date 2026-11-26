using BE.Api.Auth;
using BE.Infrastructure.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;

namespace BE.Api.Tests.Auth;

public class TokenServiceTests
{
    private static readonly JwtOptions Options = new()
    {
        Issuer = "test-issuer",
        Audience = "test-audience",
        Key = "test-signing-key-that-is-long-enough-1234567890",
        ExpiresMinutes = 60,
    };

    private sealed class FixedTime(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }

    private static TokenService Service(DateTimeOffset? now = null, JwtOptions? options = null) =>
        new(Microsoft.Extensions.Options.Options.Create(options ?? Options), new FixedTime(now ?? DateTimeOffset.UtcNow));

    private static AppUserEntity Employee() => new()
    {
        Id = Guid.NewGuid(),
        Email = "anna@businessesbjerg.com",
        Role = "Employee",
        EmployeeId = Guid.NewGuid(),
    };

    private static Task<Microsoft.IdentityModel.Tokens.TokenValidationResult> Validate(string token, JwtOptions? options = null) =>
        new JsonWebTokenHandler().ValidateTokenAsync(token, TokenService.CreateValidationParameters(options ?? Options));

    [Fact]
    public async Task Token_ContainsUserIdEmailRoleAndEmployeeId()
    {
        var user = Employee();

        var result = await Validate(Service().CreateToken(user).Token);

        Assert.True(result.IsValid);
        Assert.Equal(user.Id.ToString(), result.ClaimsIdentity.FindFirst(AuthClaims.UserId)?.Value);
        Assert.Equal(user.Email, result.ClaimsIdentity.Name);
        Assert.Equal(user.EmployeeId.ToString(), result.ClaimsIdentity.FindFirst(AuthClaims.EmployeeId)?.Value);
        Assert.True(new System.Security.Claims.ClaimsPrincipal(result.ClaimsIdentity).IsInRole("Employee"));
    }

    [Fact]
    public async Task Token_ForAccountant_HasNoEmployeeId()
    {
        var accountant = new AppUserEntity { Id = Guid.NewGuid(), Email = "acc@businessesbjerg.com", Role = "Accountant" };

        var result = await Validate(Service().CreateToken(accountant).Token);

        Assert.True(result.IsValid);
        Assert.Null(result.ClaimsIdentity.FindFirst(AuthClaims.EmployeeId));
    }

    [Fact]
    public void Token_ExpiresAfterConfiguredMinutes()
    {
        var now = new DateTimeOffset(2026, 11, 22, 12, 0, 0, TimeSpan.Zero);

        var issued = Service(now).CreateToken(Employee());

        Assert.Equal(now.AddMinutes(60), issued.ExpiresAt);
    }

    [Fact]
    public async Task ExpiredToken_IsRejected()
    {
        var twoHoursAgo = DateTimeOffset.UtcNow.AddHours(-2);

        var result = await Validate(Service(twoHoursAgo).CreateToken(Employee()).Token);

        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task TokenSignedWithAnotherKey_IsRejected()
    {
        var attacker = new JwtOptions
        {
            Issuer = Options.Issuer,
            Audience = Options.Audience,
            Key = "a-completely-different-key-of-sufficient-length-000",
        };

        var forged = Service(options: attacker).CreateToken(Employee()).Token;

        Assert.False((await Validate(forged)).IsValid);
    }

    [Fact]
    public async Task TokenForAnotherAudience_IsRejected()
    {
        var otherApp = new JwtOptions { Issuer = Options.Issuer, Audience = "other-app", Key = Options.Key };

        var token = Service(options: otherApp).CreateToken(Employee()).Token;

        Assert.False((await Validate(token)).IsValid);
    }
}
