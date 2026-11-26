using System.ComponentModel.DataAnnotations;

namespace BE.Api.Auth;

/// <summary>
/// Bound from the "Jwt" section. Issuer, Audience and lifetime live in appsettings.json;
/// the signing key is a secret and comes from user-secrets locally / an environment variable in the cloud.
/// </summary>
public class JwtOptions
{
    public const string Section = "Jwt";

    [Required] public string Issuer { get; set; } = "";
    [Required] public string Audience { get; set; } = "";

    /// <summary>HMAC-SHA256 key. At least 32 bytes (256 bits).</summary>
    [Required, MinLength(32)] public string Key { get; set; } = "";

    [Range(1, 24 * 60)] public int ExpiresMinutes { get; set; } = 60;
}
