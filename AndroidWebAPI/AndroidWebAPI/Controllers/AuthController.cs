using AndroidWebAPI.Data;
using AndroidWebAPI.Models;
using AndroidWebAPI.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AndroidWebAPI.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {

        private readonly AppDbContext _context;
        private readonly IConfiguration _config;
        private readonly IAccountRepository _accountRepository;

        public AuthController(
            AppDbContext context,
            IConfiguration config,
            IAccountRepository accountRepository)
        {
            _context = context;
            _config = config;
            _accountRepository = accountRepository;
        }

        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword(
            [FromBody] ChangePasswordDto dto)
        {
            // =====================================================
            // 1. VALIDATE REQUEST
            // =====================================================

            if (dto == null)
            {
                return BadRequest(new
                {
                    message = "Request body is required."
                });
            }

            if (string.IsNullOrWhiteSpace(dto.CurrentPassword))
            {
                return BadRequest(new
                {
                    message = "Current password is required."
                });
            }

            if (string.IsNullOrWhiteSpace(dto.NewPassword))
            {
                return BadRequest(new
                {
                    message = "New password is required."
                });
            }

            if (dto.NewPassword.Length < 8)
            {
                return BadRequest(new
                {
                    message = "New password must be at least 8 characters."
                });
            }


            // =====================================================
            // 2. GET ACCOUNT ID FROM JWT
            // =====================================================

            var accountIdClaim = User.FindFirst("AccountID")?.Value;

            if (string.IsNullOrWhiteSpace(accountIdClaim))
            {
                return Unauthorized(new
                {
                    message = "Account ID was not found in the authentication token."
                });
            }

            if (!Guid.TryParse(accountIdClaim, out Guid accountId))
            {
                return Unauthorized(new
                {
                    message = "Invalid account ID in authentication token."
                });
            }


            // =====================================================
            // 3. FIND ACCOUNT
            // =====================================================

            var account = await _accountRepository.GetByIdAsync(accountId);

            if (account == null)
            {
                return NotFound(new
                {
                    message = "Account not found."
                });
            }

            if (!account.Status)
            {
                return Unauthorized(new
                {
                    message = "This account is inactive."
                });
            }


            // =====================================================
            // 4. VERIFY CURRENT / TEMPORARY PASSWORD
            // =====================================================

            if (string.IsNullOrWhiteSpace(account.PasswordHash))
            {
                return BadRequest(new
                {
                    message = "Account does not have a valid password."
                });
            }

            bool passwordCorrect;

            try
            {
                passwordCorrect = BCrypt.Net.BCrypt.Verify(
                    dto.CurrentPassword,
                    account.PasswordHash
                );
            }
            catch
            {
                passwordCorrect = false;
            }

            if (!passwordCorrect)
            {
                return BadRequest(new
                {
                    message = "Current password is incorrect."
                });
            }


            // =====================================================
            // 5. PREVENT SAME PASSWORD
            // =====================================================

            bool samePassword;

            try
            {
                samePassword = BCrypt.Net.BCrypt.Verify(
                    dto.NewPassword,
                    account.PasswordHash
                );
            }
            catch
            {
                samePassword = false;
            }

            if (samePassword)
            {
                return BadRequest(new
                {
                    message =
                        "New password must be different from the current password."
                });
            }


            // =====================================================
            // 6. CHANGE PASSWORD
            // =====================================================

            var changed = await _accountRepository.ChangePasswordAsync(
                account.AccountID,
                dto.NewPassword
            );

            if (!changed)
            {
                return StatusCode(500, new
                {
                    message = "Unable to change password."
                });
            }


            // =====================================================
            // 7. DETERMINE USER ROLE
            // =====================================================

            string role;

            if (account.AccountType.Equals(
                "Parent",
                StringComparison.OrdinalIgnoreCase))
            {
                role = "Parent";
            }
            else if (account.AccountType.Equals(
                "Personnel",
                StringComparison.OrdinalIgnoreCase))
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u =>
                        u.UserID == account.ReferenceID);

                if (user == null)
                {
                    return NotFound(new
                    {
                        message = "Personnel record not found."
                    });
                }

                role = user.UserType;
            }
            else
            {
                role = account.AccountType;
            }


            // =====================================================
            // 8. SUCCESS
            // =====================================================

            return Ok(new
            {
                message = "Password changed successfully.",
                accountID = account.AccountID,
                referenceID = account.ReferenceID,
                accountType = account.AccountType,
                role = role,
                mustChangePassword = false
            });
        }

        // =========================================================
        // UNIFIED LOGIN
        // POST /api/auth/login
        // =========================================================

        [HttpPost("login")]
        public async Task<IActionResult> Login(
            [FromBody] LoginDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Identifier))
            {
                return BadRequest(new
                {
                    message = "Username or email is required."
                });
            }

            if (string.IsNullOrWhiteSpace(dto.Password))
            {
                return BadRequest(new
                {
                    message = "Password is required."
                });
            }

            var identifier = dto.Identifier.Trim();

            // =====================================================
            // 1. FIND ACCOUNT
            //
            // Accounts.Username holds:
            //   - the parent's email, for AccountType = "Parent"
            //   - the admin-assigned username, for AccountType = "Personnel"
            //
            // So a direct Username match covers "login by username" for
            // personnel AND "login by email" for parents. It does NOT
            // cover "login by email" for personnel, since their email
            // lives on the linked Users record, not on Accounts. The
            // two fallback lookups below handle that case.
            // =====================================================

            var account = await _context.Accounts
                .FirstOrDefaultAsync(a =>
                    a.Username == identifier);

            if (account == null)
            {
                var parentMatch = await _context.Parents
                    .FirstOrDefaultAsync(p => p.Email == identifier);

                if (parentMatch != null)
                {
                    account = await _context.Accounts.FirstOrDefaultAsync(a =>
                        a.AccountType == "Parent" &&
                        a.ReferenceID == parentMatch.ParentID);
                }
            }

            if (account == null)
            {
                var userMatch = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == identifier);

                if (userMatch != null)
                {
                    account = await _context.Accounts.FirstOrDefaultAsync(a =>
                        a.AccountType == "Personnel" &&
                        a.ReferenceID == userMatch.UserID);
                }
            }

            if (account == null)
            {
                return Unauthorized(new
                {
                    message = "Invalid username/email or password."
                });
            }

            // =====================================================
            // 2. CHECK ACCOUNT STATUS
            // =====================================================

            if (!account.Status)
            {
                return Unauthorized(new
                {
                    message = "Account is inactive."
                });
            }

            // =====================================================
            // 3. CHECK LOCK
            // =====================================================

            if (account.LockedUntil.HasValue &&
                account.LockedUntil.Value > DateTime.Now)
            {
                return Unauthorized(new
                {
                    message = "Account is temporarily locked. Please try again later."
                });
            }

            // =====================================================
            // 4. CHECK PASSWORD
            // =====================================================

            if (string.IsNullOrWhiteSpace(account.PasswordHash))
            {
                return Unauthorized(new
                {
                    message = "Invalid username/email or password."
                });
            }

            bool passwordValid;

            try
            {
                passwordValid = BCrypt.Net.BCrypt.Verify(
                    dto.Password,
                    account.PasswordHash
                );
            }
            catch
            {
                passwordValid = false;
            }

            if (!passwordValid)
            {
                account.FailedLoginAttempts++;

                if (account.FailedLoginAttempts >= 5)
                {
                    account.LockedUntil =
                        DateTime.Now.AddMinutes(15);

                    account.FailedLoginAttempts = 0;
                }

                await _context.SaveChangesAsync();

                return Unauthorized(new
                {
                    message = "Invalid username/email or password."
                });
            }

            // =====================================================
            // 5. SUCCESSFUL LOGIN
            // =====================================================

            account.FailedLoginAttempts = 0;
            account.LockedUntil = null;
            account.LastLogin = DateTime.Now;

            await _context.SaveChangesAsync();

            // =====================================================
            // 6. PARENT ACCOUNT
            // =====================================================

            if (account.AccountType == "Parent")
            {
                var parent = await _context.Parents
                    .FirstOrDefaultAsync(p =>
                        p.ParentID == account.ReferenceID);

                if (parent == null)
                {
                    return Unauthorized(new
                    {
                        message = "Parent profile not found."
                    });
                }

                var token = GenerateJwtToken(
                    account,
                    parent.ParentID,
                    parent.Email,
                    "Parent"
                );

                return Ok(new
                {
                    token,

                    accountID = account.AccountID,

                    referenceID = account.ReferenceID,

                    accountType = account.AccountType,

                    role = "Parent",

                    user = new
                    {
                        parentID = parent.ParentID,
                        firstName = parent.FirstName,
                        middleName = parent.MiddleName,
                        lastName = parent.LastName,
                        email = parent.Email,
                        contactNo = parent.ContactNo,
                        barangayNo = parent.BarangayNo,
                        address = parent.Address
                    },

                    mustChangePassword =
                        account.MustChangePassword,

                    temporaryPasswordExpiresAt =
                        parent.TemporaryPasswordExpiresAt,

                    lastLogin = account.LastLogin
                });
            }

            // =====================================================
            // 7. PERSONNEL ACCOUNT
            // =====================================================

            if (account.AccountType == "Personnel")
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u =>
                        u.UserID == account.ReferenceID);

                if (user == null)
                {
                    return Unauthorized(new
                    {
                        message = "Personnel profile not found."
                    });
                }

                var token = GenerateJwtToken(
                    account,
                    user.UserID,
                    user.Email,
                    user.UserType
                );

                return Ok(new
                {
                    token,

                    accountID = account.AccountID,

                    referenceID = account.ReferenceID,

                    accountType = account.AccountType,

                    role = user.UserType,

                    user = new
                    {
                        userID = user.UserID,
                        firstName = user.FirstName,
                        middleName = user.MiddleName,
                        lastName = user.LastName,
                        username = user.Username,
                        email = user.Email,
                        contactNo = user.ContactNo,
                        prcNo = user.PRCNo,
                        accountStatus = user.AccountStatus
                    },

                    mustChangePassword =
                        account.MustChangePassword,

                    temporaryPasswordExpiresAt =
                        (DateTime?)null,

                    lastLogin = account.LastLogin
                });
            }

            return Unauthorized(new
            {
                message = "Unsupported account type."
            });
        }

        // =========================================================
        // JWT
        // =========================================================

        private string GenerateJwtToken(
            Account account,
            Guid referenceID,
            string? email,
            string role)
        {
            var key = GetJwtKey();

            var credentials =
                new SigningCredentials(
                    key,
                    SecurityAlgorithms.HmacSha256
                );

            var claims = new List<Claim>
            {
                new Claim(
                    ClaimTypes.NameIdentifier,
                    account.AccountID.ToString()
                ),

                new Claim(
                    ClaimTypes.Role,
                    role
                ),

                new Claim(
                    "AccountID",
                    account.AccountID.ToString()
                ),

                new Claim(
                    "ReferenceID",
                    referenceID.ToString()
                ),

                new Claim(
                    "AccountType",
                    account.AccountType
                )
            };

            if (!string.IsNullOrWhiteSpace(email))
            {
                claims.Add(
                    new Claim(
                        ClaimTypes.Email,
                        email
                    )
                );
            }

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(8),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler()
                .WriteToken(token);
        }

        // =========================================================
        // JWT KEY
        // =========================================================

        private SymmetricSecurityKey GetJwtKey()
        {
            var jwtKey = _config["Jwt:Key"];

            if (string.IsNullOrWhiteSpace(jwtKey))
            {
                throw new InvalidOperationException(
                    "JWT Key is not configured."
                );
            }

            return new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey)
            );
        }
    }

    // =============================================================
    // LOGIN DTO
    // =============================================================

    public class LoginDto
    {
        public string Identifier { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;
    }


}   