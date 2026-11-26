using Microsoft.AspNetCore.Identity;

namespace BE.Api.Auth;

public enum PasswordCheck
{
    Failed,
    Success,

    /// <summary>Correct password, but hashed with older settings – store a fresh hash.</summary>
    SuccessRehashNeeded,
}

/// <summary>
/// Wraps ASP.NET Core Identity's PasswordHasher: PBKDF2 with HMAC-SHA512, 100 000 iterations,
/// a random 128-bit salt per password and a constant-time comparison. We never store or log plain passwords.
/// </summary>
public class PasswordService
{
    private static readonly PasswordHasher<object> Hasher = new();
    private static readonly object User = new(); // the default hasher does not use the user object

    // Used when the email does not exist, so a wrong email takes as long as a wrong password
    // (otherwise response times would reveal which emails have an account).
    private static readonly string DummyHash = Hasher.HashPassword(User, "not-a-real-password-just-for-timing");

    public string Hash(string password) => Hasher.HashPassword(User, password);

    public PasswordCheck Verify(string? storedHash, string password)
    {
        var result = Hasher.VerifyHashedPassword(User, storedHash ?? DummyHash, password);
        if (storedHash is null) return PasswordCheck.Failed;

        return result switch
        {
            PasswordVerificationResult.Success => PasswordCheck.Success,
            PasswordVerificationResult.SuccessRehashNeeded => PasswordCheck.SuccessRehashNeeded,
            _ => PasswordCheck.Failed,
        };
    }
}
