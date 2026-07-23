// Controllers/AuthController.cs
// Handles login for Staff, Doctor, and Nurse (dbo.Users)
// Parent login is separate (dbo.Parents)

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AndroidWebAPI.Data;   // ← ADD THIS — matches wherever AppDbContext.cs lives
using AndroidWebAPI.Models; // ← ADD THIS — for the User model

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _config;

    public AuthController(AppDbContext context, IConfiguration config)
    {
        _context = context;
        _config = config;
    }

    // POST /api/auth/staff-login
    // Used by: StaffLogin.vue (Doctor, Nurse, Staff, Admin)
    [HttpPost("staff-login")]
    public async Task<IActionResult> StaffLogin([FromBody] StaffLoginDto dto)
    {
        // Find user by username
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Username == dto.Username && u.AccountStatus == "Active");

        if (user == null)
            return Unauthorized(new { message = "Invalid username or password." });

        // ── PASSWORD CHECK ──────────────────────────────────────────
        // OPTION A: Plain text check (temporary, for development only)
        // Remove this once you implement BCrypt hashing
        bool passwordValid = user.PasswordHash == dto.Password;

        // OPTION B: BCrypt check (use this in production)
        // Install: dotnet add package BCrypt.Net-Next
        // bool passwordValid = BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);

        if (!passwordValid)
            return Unauthorized(new { message = "Invalid username or password." });
        // ────────────────────────────────────────────────────────────

        // Generate JWT token
        var token = GenerateJwtToken(user);

        return Ok(new {
            token,
            user = new {
                userID    = user.UserID,
                firstName = user.FirstName,
                lastName  = user.LastName,
                userType  = user.UserType,   // 'Doctor' | 'Nurse' | 'Staff' | 'Admin'
                prcNo     = user.PRCNo,
            }
        });
    }

    private string GenerateJwtToken(User user)
    {
        var key     = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var creds   = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddHours(8); // token valid for one clinic shift

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.UserID.ToString()),
            new Claim(ClaimTypes.Name,           $"{user.FirstName} {user.LastName}"),
            new Claim(ClaimTypes.Role,           user.UserType),
        };

        var token = new JwtSecurityToken(
            issuer:   _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims:   claims,
            expires:  expires,
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

// DTO for the login request body
public class StaffLoginDto
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

// ── appsettings.json — add this section ─────────────────────
// "Jwt": {
//   "Key": "YourSuperSecretKeyAtLeast32CharsLong!",
//   "Issuer": "ArugaSystem",
//   "Audience": "ArugaSystem"
// }