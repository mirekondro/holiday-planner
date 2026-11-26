using BE.Api.Auth;
using BE.Api.Contracts;
using BE.Domain.Users;
using BE.Infrastructure.Database;
using BE.Infrastructure.Entities;
using LinqToDB;
using LinqToDB.Async;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace BE.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(AppDataConnection db, PasswordService passwords, TokenService tokens) : ControllerBase
{
    /// <summary>Exchanges email + password for a JWT.</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimits.Login)]
    [ProducesResponseType<LoginResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request)
    {
        var email = UserRules.NormalizeEmail(request.Email);
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email);

        // Same check and same answer whether the email or the password is wrong – no account enumeration.
        var check = passwords.Verify(user?.PasswordHash, request.Password);
        if (user is null || check == PasswordCheck.Failed)
            return Unauthorized(new ProblemDetails { Title = "Invalid email or password." });

        if (check == PasswordCheck.SuccessRehashNeeded)
            await db.Users.Where(u => u.Id == user.Id).Set(u => u.PasswordHash, passwords.Hash(request.Password)).UpdateAsync();

        var token = tokens.CreateToken(user);
        return new LoginResponse(token.Token, token.ExpiresAt, ToCurrentUser(user));
    }

    /// <summary>Who am I? Lets the client restore the session from a stored token.</summary>
    [HttpGet("me")]
    [ProducesResponseType<CurrentUserDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult<CurrentUserDto>> Me()
    {
        if (!Guid.TryParse(User.FindFirst(AuthClaims.UserId)?.Value, out var userId))
            return Unauthorized();

        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user is null) return Unauthorized();
        return ToCurrentUser(user);
    }

    private static CurrentUserDto ToCurrentUser(AppUserEntity user) =>
        new(user.Id, user.Email, Enum.Parse<UserRole>(user.Role), user.EmployeeId);
}
