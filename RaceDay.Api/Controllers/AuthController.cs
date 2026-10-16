using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.DTOs;
using RaceDay.Api.Models;
using RaceDay.Api.Services;
namespace RaceDay.Api.Controllers;
[ApiController, Route("api/auth")]
public class AuthController(RaceDayDbContext db, IPasswordHasher<UserAccount> hasher, JwtService jwt) : ControllerBase {
    /// <summary>Register an Organiser or Participant account. Passwords are hashed before storage.</summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request) {
        var role = request.Role.Trim();
        if (!RaceDayRules.IsValidRole(role)) return BadRequest(new { message = "Role must be Organiser or Participant." });
        var email = request.Email.Trim().ToLowerInvariant();
        if (await db.Users.AnyAsync(x => x.Email == email)) return Conflict(new { message = "An account with this email already exists." });
        var user = new UserAccount { FirstName = request.FirstName.Trim(), LastName = request.LastName.Trim(), Email = email, Role = role };
        user.PasswordHash = hasher.HashPassword(user, request.Password);
        db.Users.Add(user); await db.SaveChangesAsync();
        return Created("/api/profile", new { user.UserId, user.FirstName, user.LastName, user.Email, user.Role });
    }
    /// <summary>Log in and receive a bearer token for subsequent requests.</summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request) {
        var user = await db.Users.SingleOrDefaultAsync(x => x.Email == request.Email.Trim().ToLowerInvariant());
        if (user is null || hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password) == PasswordVerificationResult.Failed) return Unauthorized(new { message = "Invalid email or password." });
        return Ok(new { token = jwt.CreateToken(user), userId = user.UserId, user.FirstName, user.LastName, user.Email, user.Role, tokenType = "Bearer" });
    }
}
