namespace BE.Domain.Users;

/// <summary>Business rules for login accounts. Pure functions, so they are easy to unit test.</summary>
public static class UserRules
{
    public const string OrganizationDomain = "@businessesbjerg.com";
    public const int MinPasswordLength = 12;
    public const int MaxPasswordLength = 128;

    /// <summary>Emails are compared and stored in lower case without surrounding spaces.</summary>
    public static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();

    public static bool IsOrganizationEmail(string email)
    {
        var normalized = NormalizeEmail(email);
        // Something must come before the domain, and the domain must be the very end.
        return normalized.Length > OrganizationDomain.Length
               && normalized.EndsWith(OrganizationDomain, StringComparison.Ordinal)
               && normalized.IndexOf('@') == normalized.Length - OrganizationDomain.Length;
    }

    /// <summary>
    /// Length-based policy (NIST SP 800-63B): long passwords instead of forced character classes.
    /// The upper limit stops very long inputs from being used to slow down hashing.
    /// </summary>
    public static bool IsAcceptablePassword(string password) =>
        password.Length is >= MinPasswordLength and <= MaxPasswordLength
        && !string.IsNullOrWhiteSpace(password);

    /// <summary>Only employee accounts are linked to an employee; accountant and system owner are not.</summary>
    public static bool HasValidEmployeeLink(UserRole role, Guid? employeeId) =>
        role == UserRole.Employee ? employeeId is not null : employeeId is null;

    /// <summary>The organisation has exactly one accountant and one system owner account.</summary>
    public static bool IsSingleAccountRole(UserRole role) => role is UserRole.Accountant or UserRole.SystemOwner;
}
