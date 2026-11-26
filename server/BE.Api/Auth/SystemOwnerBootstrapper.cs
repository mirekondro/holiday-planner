using BE.Domain.Users;
using BE.Infrastructure.Database;
using BE.Infrastructure.Entities;
using LinqToDB;
using LinqToDB.Async;

namespace BE.Api.Auth;

/// <summary>
/// Only the system owner can create users, so the very first system owner has to come from somewhere.
/// On startup: if no system owner exists and Bootstrap:SystemOwnerEmail / Bootstrap:SystemOwnerPassword
/// are configured (user-secrets or environment variables – never git), that account is created.
/// </summary>
public static class SystemOwnerBootstrapper
{
    public static async Task EnsureSystemOwnerAsync(IServiceProvider services, IConfiguration config, ILogger logger)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDataConnection>();
        var passwords = scope.ServiceProvider.GetRequiredService<PasswordService>();

        var ownerRole = nameof(UserRole.SystemOwner);
        if (await db.Users.AnyAsync(u => u.Role == ownerRole))
            return;

        var email = config["Bootstrap:SystemOwnerEmail"];
        var password = config["Bootstrap:SystemOwnerPassword"];
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            logger.LogWarning("No system owner exists. Set Bootstrap:SystemOwnerEmail and Bootstrap:SystemOwnerPassword to create one.");
            return;
        }

        if (!UserRules.IsOrganizationEmail(email) || !UserRules.IsAcceptablePassword(password))
            throw new InvalidOperationException(
                $"Bootstrap system owner needs a {UserRules.OrganizationDomain} email and a password of at least {UserRules.MinPasswordLength} characters.");

        await db.InsertAsync(new AppUserEntity
        {
            Id = Guid.NewGuid(),
            Email = UserRules.NormalizeEmail(email),
            PasswordHash = passwords.Hash(password),
            Role = ownerRole,
            EmployeeId = null,
            CreatedAt = DateTimeOffset.UtcNow,
        });
        logger.LogInformation("Created the system owner account {Email}.", UserRules.NormalizeEmail(email));
    }
}
