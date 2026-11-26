namespace BE.Domain.Users;

public enum UserRole
{
    /// <summary>Every person in the organisation. Applies for holiday and sees the shared calendar.</summary>
    Employee,

    /// <summary>The single accountant account. Approves and declines requests, manages work schedules.</summary>
    Accountant,

    /// <summary>The single system owner account. Creates users, defines global rules and schedules.</summary>
    SystemOwner,
}
