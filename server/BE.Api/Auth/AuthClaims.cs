namespace BE.Api.Auth;

/// <summary>Claim names used in our JWTs (short JWT-style names, no Microsoft URL-style mapping).</summary>
public static class AuthClaims
{
    public const string UserId = "sub";
    public const string Email = "email";
    public const string Role = "role";
    public const string EmployeeId = "employee_id";
}
