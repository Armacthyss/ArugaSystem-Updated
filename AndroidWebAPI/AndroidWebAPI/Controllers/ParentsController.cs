using AndroidWebAPI.Data;
using AndroidWebAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace AndroidWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ParentsController : ControllerBase
    {
        private readonly ParentRepository _parentRepo;

        public ParentsController(ParentRepository parentRepo)
        {
            _parentRepo = parentRepo;
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
                if (string.IsNullOrWhiteSpace(dto.Password) || dto.Password.Length < 6)
                    return BadRequest(new { message = "Password must be at least 6 characters." });
                if (string.IsNullOrWhiteSpace(dto.ContactNo))
                    return BadRequest(new { message = "ContactNo is required." });

                var parent = new Parent
                {
                    ParentID = Guid.NewGuid(),
                    FirstName = dto.FirstName,
                    MiddleName = dto.MiddleName ?? string.Empty,
                    LastName = dto.LastName,
                    Email = dto.Email,
                    ContactNo = dto.ContactNo,
                    BarangayNo = dto.BarangayNo ?? string.Empty,
                    Address = dto.Address ?? string.Empty,
                    Password = dto.Password // Will be hashed in CreateAsync
                };

                var created = await _parentRepo.CreateAsync(parent);
                return CreatedAtAction(nameof(GetParentById), new { id = created.ParentID }, new
                {
                    parentID = created.ParentID,
                    firstName = created.FirstName,
                    middleName = created.MiddleName,
                    lastName = created.LastName,
                    email = created.Email,
                    contactNo = created.ContactNo,
                    barangayNo = created.BarangayNo,
                    address = created.Address
                });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred: " + ex.Message });
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
                address = parent.Address
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
                    Password = existing.Password // Don't update password here
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
                    address = updated.Address
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred: " + ex.Message });
            }
        }

        // ── LOGIN: POST /api/Parents/login ────────────────────────
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var parent = await _parentRepo.LoginAsync(request.Email, request.Password);
            if (parent == null)
                return Unauthorized(new { message = "Invalid email or password" });

            return Ok(new
            {
                parentID = parent.ParentID,
                firstName = parent.FirstName,
                middleName = parent.MiddleName,
                lastName = parent.LastName,
                email = parent.Email,
                contactNo = parent.ContactNo,
                barangayNo = parent.BarangayNo,
                address = parent.Address
            });
        }

        // ── CHANGE PASSWORD: PATCH /api/Parents/{id}/change-password
        [HttpPatch("{id}/change-password")]
        public async Task<IActionResult> ChangePassword(Guid id, [FromBody] ChangePasswordDto dto)
        {
            var success = await _parentRepo.ChangePasswordAsync(id, dto.CurrentPassword, dto.NewPassword);
            if (!success)
                return BadRequest(new { message = "Current password is incorrect." });
            return Ok(new { message = "Password updated." });
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

    // ── DTOs ──────────────────────────────────────────────────────
    public class CreateParentDto
    {
        public string FirstName { get; set; } = string.Empty;
        public string? MiddleName { get; set; }
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string ContactNo { get; set; } = string.Empty;
        public string? BarangayNo { get; set; }
        public string? Address { get; set; }
    }

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

    public class ChangePasswordDto
    {
        public string CurrentPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }
}