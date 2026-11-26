using System.ComponentModel.DataAnnotations;
using BE.Domain.Users;

namespace BE.Api.Contracts;

public record LoginRequest(
    [Required, EmailAddress, MaxLength(254)] string Email,
    [Required, MaxLength(UserRules.MaxPasswordLength)] string Password);

public record LoginResponse(string Token, DateTimeOffset ExpiresAt, CurrentUserDto User);

public record CurrentUserDto(Guid Id, string Email, UserRole Role, Guid? EmployeeId);

public record CreateUserRequest(
    [Required, EmailAddress, MaxLength(254)] string Email,
    [Required, MaxLength(UserRules.MaxPasswordLength)] string Password,
    [Required] UserRole? Role, // nullable so a missing role is a 400, not silently "Employee"
    Guid? EmployeeId);

public record UserDto(Guid Id, string Email, UserRole Role, Guid? EmployeeId, DateTimeOffset CreatedAt);
