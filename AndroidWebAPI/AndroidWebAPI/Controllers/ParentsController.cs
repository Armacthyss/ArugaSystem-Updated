using AndroidWebAPI.Data;
using AndroidWebAPI.DTOs;
using AndroidWebAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace AndroidWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ParentsController : ControllerBase
    {
        private readonly ParentRepository _parentRepo;
        private readonly IAccountRepository _accountRepository;

        public ParentsController(
            ParentRepository parentRepo,
            IAccountRepository accountRepository)
        {
            _parentRepo = parentRepo;
            _accountRepository = accountRepository;
        }

        private static string GenerateTemporaryPassword()
        {
            const string characters =
                "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz23456789";

            var random = new Random();

            return new string(
                Enumerable
                    .Range(0, 10)
                    .Select(_ => characters[random.Next(characters.Length)])
                    .ToArray()
            );
        }

        // ── CREATE: POST /api/Parents ─────────────────────────────
        [HttpPost]
        public async Task<IActionResult> CreateParent([FromBody] CreateParentDto dto)
        {
            try
            {
                // Validation
                if (string.IsNullOrWhiteSpace(dto.FirstName))
                    return BadRequest(new { message = "FirstName is required." });

                if (string.IsNullOrWhiteSpace(dto.LastName))
                    return BadRequest(new { message = "LastName is required." });

                if (string.IsNullOrWhiteSpace(dto.Email))
                    return BadRequest(new { message = "Email is required." });

                if (string.IsNullOrWhiteSpace(dto.ContactNo))
                    return BadRequest(new { message = "ContactNo is required." });

                // Accounts.Username = the parent's email for this account type,
                // so it must be unique across ALL accounts (parent + personnel).
                var existingAccount =
                    await _accountRepository.GetByUsernameAsync(dto.Email);

                if (existingAccount != null)
                {
                    return Conflict(new
                    {
                        message = "An account with this email already exists."
                    });
                }

                // Generate temporary password
                string temporaryPassword = GenerateTemporaryPassword();

                // Hash temporary password
                string passwordHash =
                    BCrypt.Net.BCrypt.HashPassword(temporaryPassword);

                var parent = new Parent
                {
                    ParentID = Guid.NewGuid(),

                    FirstName = dto.FirstName,
                    MiddleName = dto.MiddleName,
                    LastName = dto.LastName,

                    Email = dto.Email,
                    ContactNo = dto.ContactNo,

                    BarangayNo = dto.BarangayNo,
                    Address = dto.Address,

                    PasswordHash = passwordHash,

                    // Force password change after first login
                    MustChangePassword = true,

                    // Temporary password valid for 24 hours
                    TemporaryPasswordExpiresAt = DateTime.Now.AddHours(24)
                };

                var created = await _parentRepo.CreateAsync(parent);

                // ─────────────────────────────────────────────
                // Create the matching Accounts row so this parent
                // can log in through /api/auth/login like every
                // other account type.
                // ─────────────────────────────────────────────

                var account = new Account
                {
                    AccountID = Guid.NewGuid(),
                    Username = dto.Email,
                    PasswordHash = passwordHash,
                    AccountType = "Parent",
                    ReferenceID = created.ParentID,
                    Status = true,
                    MustChangePassword = true,
                    FailedLoginAttempts = 0,
                    LockedUntil = null,
                    LastLogin = null,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = null
                };

                await _accountRepository.CreateAsync(account);

                return CreatedAtAction(
                    nameof(GetParentById),
                    new { id = created.ParentID },
                    new
                    {
                        parentID = created.ParentID,
                        accountID = account.AccountID,
                        firstName = created.FirstName,
                        middleName = created.MiddleName,
                        lastName = created.LastName,
                        email = created.Email,
                        contactNo = created.ContactNo,
                        barangayNo = created.BarangayNo,
                        address = created.Address,

                        mustChangePassword = created.MustChangePassword,
                        temporaryPasswordExpiresAt =
                            created.TemporaryPasswordExpiresAt,

                        // TESTING ONLY — shown to the admin so they can
                        // hand it to the parent. Remove once email delivery exists.
                        temporaryPassword = temporaryPassword
                    }
                );
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(
                    500,
                    new
                    {
                        message = "An error occurred: " + ex.Message
                    }
                );
            }
        }

        // ── READ: GET /api/Parents/{id} ───────────────────────────
        [HttpGet("{id}")]
        public async Task<IActionResult> GetParentById(Guid id)
        {
            var parent = await _parentRepo.GetByIdAsync(id);
            if (parent == null)
                return NotFound(new { message = "Parent not found" });

            return Ok(new
            {
                parentID = parent.ParentID,

                firstName = parent.FirstName,
                middleName = parent.MiddleName,
                lastName = parent.LastName,

                email = parent.Email,
                contactNo = parent.ContactNo,
                barangayNo = parent.BarangayNo,
                address = parent.Address,

                mustChangePassword = parent.MustChangePassword,
                temporaryPasswordExpiresAt =
                    parent.TemporaryPasswordExpiresAt,

                lastLogin = parent.LastLogin,

                role = "Parent"

            });
        }

        // ── READ: GET /api/Parents/all ────────────────────────────
        [HttpGet("all")]
        public async Task<IActionResult> GetAllParents()
        {
            var parents = await _parentRepo.GetAllAsync();
            var result = parents.Select(p => new
            {
                parentID = p.ParentID,
                firstName = p.FirstName,
                middleName = p.MiddleName,
                lastName = p.LastName,
                email = p.Email,
                contactNo = p.ContactNo,
                barangayNo = p.BarangayNo,
                address = p.Address
            });

            return Ok(result);
        }

        // ── UPDATE: PUT /api/Parents/{id} ─────────────────────────
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateParent(Guid id, [FromBody] UpdateParentDto dto)
        {
            try
            {
                // Get existing parent to verify it exists
                var existing = await _parentRepo.GetByIdAsync(id);
                if (existing == null)
                    return NotFound(new { message = "Parent not found" });

                // Validate required fields
                if (string.IsNullOrWhiteSpace(dto.FirstName))
                    return BadRequest(new { message = "FirstName is required." });
                if (string.IsNullOrWhiteSpace(dto.LastName))
                    return BadRequest(new { message = "LastName is required." });

                var parent = new Parent
                {
                    ParentID = id,
                    FirstName = dto.FirstName,
                    MiddleName = dto.MiddleName ?? existing.MiddleName,
                    LastName = dto.LastName,
                    Email = dto.Email ?? existing.Email,
                    ContactNo = dto.ContactNo ?? existing.ContactNo,
                    BarangayNo = dto.BarangayNo ?? existing.BarangayNo,
                    Address = dto.Address ?? existing.Address,
                    PasswordHash = existing.PasswordHash // Don't update password here
                };

                var updated = await _parentRepo.UpdateAsync(parent);
                return Ok(new
                {
                    parentID = updated.ParentID,
                    firstName = updated.FirstName,
                    middleName = updated.MiddleName,
                    lastName = updated.LastName,
                    email = updated.Email,
                    contactNo = updated.ContactNo,
                    barangayNo = updated.BarangayNo,
                    address = updated.Address,

                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred: " + ex.Message });
            }
        }

        // ── LOGIN: POST /api/Parents/login ────────────────────────
        // NOTE: This is a legacy, parallel login path that checks
        // Parents.PasswordHash directly and is NOT used by Login.vue
        // (which calls /api/auth/login instead). Now that parents also
        // have an Accounts row, consider retiring this endpoint —
        // it will drift out of sync with Accounts.PasswordHash after
        // any password change or admin reset done via /api/accounts
        // or /api/auth/change-password.
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var parent = await _parentRepo.LoginAsync(
                request.Email,
                request.Password
            );

            if (parent == null)
                return Unauthorized(new
                {
                    message = "Invalid email or password."
                });

            // Check temporary password expiration
            if (
                parent.MustChangePassword &&
                parent.TemporaryPasswordExpiresAt.HasValue &&
                parent.TemporaryPasswordExpiresAt.Value < DateTime.Now
            )
            {
                return Unauthorized(new
                {
                    message = "Your temporary password has expired. Please contact the health center."
                });
            }

            return Ok(new
            {
                parentID = parent.ParentID,

                firstName = parent.FirstName,
                middleName = parent.MiddleName,
                lastName = parent.LastName,

                email = parent.Email,
                contactNo = parent.ContactNo,
                barangayNo = parent.BarangayNo,
                address = parent.Address,

                mustChangePassword = parent.MustChangePassword,
                temporaryPasswordExpiresAt =
                    parent.TemporaryPasswordExpiresAt,

                lastLogin = parent.LastLogin,

                role = "Parent"
            });
        }

        // ── CHANGE PASSWORD: PATCH /api/Parents/{id}/change-password
        // NOTE: same drift risk as Login above — this only updates
        // Parents.PasswordHash, not Accounts.PasswordHash.
        [HttpPatch("{id}/change-password")]
        public async Task<IActionResult> ChangePassword(
            Guid id,
            [FromBody] ChangePasswordDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.CurrentPassword))
                return BadRequest(new
                {
                    message = "Current password is required."
                });

            if (string.IsNullOrWhiteSpace(dto.NewPassword))
                return BadRequest(new
                {
                    message = "New password is required."
                });

            if (dto.NewPassword.Length < 8)
                return BadRequest(new
                {
                    message = "New password must be at least 8 characters."
                });

            var result = await _parentRepo.ChangePasswordAsync(
                id,
                dto.CurrentPassword,
                dto.NewPassword
            );

            if (!result.Success)
                return BadRequest(new
                {
                    message = result.Message
                });

            return Ok(new
            {
                message = result.Message
            });
        }

        // ── DASHBOARD: GET /api/Parents/dashboard/{id} ────────────
        [HttpGet("dashboard/{id}")]
        public async Task<IActionResult> GetDashboard(Guid id)
        {
            var data = await _parentRepo.GetDashboardData(id);
            if (data == null)
                return NotFound(new { message = "Parent not found" });
            return Ok(data);
        }
    }

    // ── DTOs still used only by this controller ──────────────────
    public class UpdateParentDto
    {
        public string FirstName { get; set; } = string.Empty;
        public string? MiddleName { get; set; }
        public string LastName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? ContactNo { get; set; }
        public string? BarangayNo { get; set; }
        public string? Address { get; set; }
    }

    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}