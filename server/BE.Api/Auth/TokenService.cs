using System.Security.Claims;
using System.Text;
using BE.Infrastructure.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace BE.Api.Auth;

public record IssuedToken(string Token, DateTimeOffset ExpiresAt);

public class TokenService(IOptions<JwtOptions> options, TimeProvider time)
{
    private readonly JwtOptions _options = options.Value;

    public IssuedToken CreateToken(AppUserEntity user)
    {
        var now = time.GetUtcNow();
        var expiresAt = now.AddMinutes(_options.ExpiresMinutes);

        var claims = new List<Claim>
        {
            new(AuthClaims.UserId, user.Id.ToString()),
            new(AuthClaims.Email, user.Email),
            new(AuthClaims.Role, user.Role),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };
        if (user.EmployeeId is { } employeeId)
            claims.Add(new Claim(AuthClaims.EmployeeId, employeeId.ToString()));

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Issuer = _options.Issuer,
            Audience = _options.Audience,
            IssuedAt = now.UtcDateTime,
            NotBefore = now.UtcDateTime,
            Expires = expiresAt.UtcDateTime,
            SigningCredentials = new SigningCredentials(CreateSigningKey(_options.Key), SecurityAlgorithms.HmacSha256),
        };

        return new IssuedToken(new JsonWebTokenHandler().CreateToken(descriptor), expiresAt);
    }

    public static SymmetricSecurityKey CreateSigningKey(string key) => new(Encoding.UTF8.GetBytes(key));

    /// <summary>The same rules are used to issue tokens (above) and to validate incoming ones (Program.cs).</summary>
    public static TokenValidationParameters CreateValidationParameters(JwtOptions options) => new()
    {
        ValidateIssuer = true,
        ValidIssuer = options.Issuer,
        ValidateAudience = true,
        ValidAudience = options.Audience,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = CreateSigningKey(options.Key),
        ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.FromSeconds(30),
        NameClaimType = AuthClaims.Email,
        RoleClaimType = AuthClaims.Role,
    };
}
