using BE.Api.Auth;
using BE.Api.Contracts;
using BE.Domain.Users;
using BE.Infrastructure.Database;
using BE.Infrastructure.Entities;
using LinqToDB;
using LinqToDB.Async;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BE.Api.Controllers;

/// <summary>User administration. Only the system owner may create or list login accounts.</summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = Policies.SystemOwner)]
public class UsersController(AppDataConnection db, PasswordService passwords) : ControllerBase
{
    [HttpGet]
    public async Task<List<UserDto>> GetAll()
    {
        var users = await db.Users.OrderBy(u => u.Email).ToListAsync();
        return users.Select(u => new UserDto(u.Id, u.Email, Enum.Parse<UserRole>(u.Role), u.EmployeeId, u.CreatedAt)).ToList();
    }

    [HttpPost]
    [ProducesResponseType<UserDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UserDto>> Create(CreateUserRequest request)
    {
        var requestedRole = request.Role!.Value; // [Required] already rejected a missing role with 400

        // Server-side validation of business rules (the [Required]/[EmailAddress] attributes run before this).
        if (!UserRules.IsOrganizationEmail(request.Email))
            ModelState.AddModelError(nameof(request.Email), $"Email must end with {UserRules.OrganizationDomain}.");
        if (!UserRules.IsAcceptablePassword(request.Password))
            ModelState.AddModelError(nameof(request.Password),
                $"Password must be {UserRules.MinPasswordLength}–{UserRules.MaxPasswordLength} characters.");
        if (!UserRules.HasValidEmployeeLink(requestedRole, request.EmployeeId))
            ModelState.AddModelError(nameof(request.EmployeeId),
                requestedRole == UserRole.Employee
                    ? "An employee account must be linked to an employee."
                    : "Accountant and system owner accounts are not linked to an employee.");
        if (request.EmployeeId is { } employeeId && !await db.Employees.AnyAsync(e => e.Id == employeeId))
            ModelState.AddModelError(nameof(request.EmployeeId), "Employee does not exist.");
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var email = UserRules.NormalizeEmail(request.Email);
        var role = requestedRole.ToString();

        if (await db.Users.AnyAsync(u => u.Email == email))
            return Conflict(new ProblemDetails { Title = "A user with this email already exists." });
        if (UserRules.IsSingleAccountRole(requestedRole) && await db.Users.AnyAsync(u => u.Role == role))
            return Conflict(new ProblemDetails { Title = $"There can only be one {role} account." });
        if (request.EmployeeId is { } id && await db.Users.AnyAsync(u => u.EmployeeId == id))
            return Conflict(new ProblemDetails { Title = "This employee already has a login." });

        var user = new AppUserEntity
        {
            Id = Guid.NewGuid(),
            Email = email,
            PasswordHash = passwords.Hash(request.Password),
            Role = role,
            EmployeeId = request.EmployeeId,
            CreatedAt = DateTimeOffset.UtcNow,
        };
        // The unique indexes in 0003_app_user.sql are the final guard if two requests race each other.
        await db.InsertAsync(user);

        var dto = new UserDto(user.Id, user.Email, requestedRole, user.EmployeeId, user.CreatedAt);
        return Created($"/api/users/{user.Id}", dto);
    }
}
