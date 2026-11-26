using BE.Domain.Users;

namespace BE.Api.Auth;

/// <summary>
/// Named authorization policies. Controllers use these names, never role strings directly,
/// so the "who can access what" table in README.md maps 1:1 to this file.
/// </summary>
public static class Policies
{
    /// <summary>Applies for holiday, cancellation and transfer.</summary>
    public const string Employee = nameof(Employee);

    /// <summary>Approves and declines requests.</summary>
    public const string Accountant = nameof(Accountant);

    /// <summary>Creates users and edits global rules.</summary>
    public const string SystemOwner = nameof(SystemOwner);

    /// <summary>Work schedules may be edited by the accountant or the system owner, never by the employee.</summary>
    public const string ManageSchedules = nameof(ManageSchedules);

    public static void AddHolidayPlannerPolicies(this Microsoft.AspNetCore.Authorization.AuthorizationOptions options)
    {
        options.AddPolicy(Employee, p => p.RequireRole(nameof(UserRole.Employee)));
        options.AddPolicy(Accountant, p => p.RequireRole(nameof(UserRole.Accountant)));
        options.AddPolicy(SystemOwner, p => p.RequireRole(nameof(UserRole.SystemOwner)));
        options.AddPolicy(ManageSchedules, p => p.RequireRole(nameof(UserRole.Accountant), nameof(UserRole.SystemOwner)));

        // Secure by default: every endpoint requires a logged-in user unless it says [AllowAnonymous].
        options.FallbackPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .Build();
    }
}
