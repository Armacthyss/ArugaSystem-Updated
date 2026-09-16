using AndroidWebAPI.Data;
using AndroidWebAPI.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace AndroidWebAPI.Controllers
{

    
    [ApiController]

    
    [Route("api/[controller]")]
    [Authorize]
    public class HealthcareController : ControllerBase
    {
        private readonly AppDbContext _context;

        public HealthcareController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("workers")]
public async Task<IActionResult> GetHealthcareWorkers()
{
    var workers = await _context.Users
        .Where(u =>
            u.AccountStatus == "Active" &&
            (
                u.Position == "Doctor" ||
                u.Position == "Nurse" ||
                u.Position == "Nurse/Midwife"
            ))
        .Select(u => new
        {
            userID = u.UserID,
            firstName = u.FirstName,
            lastName = u.LastName,
            position = u.Position,
            availabilityStatus = u.AvailabilityStatus
        })
        .ToListAsync();

    return Ok(workers);
}

        // PUT: api/Healthcare/availability
        [HttpPut("availability")]
        public async Task<IActionResult> UpdateAvailability(
            [FromBody] UpdateAvailabilityRequest request)
        {
            // Get the logged-in user's UserID from the JWT
            var referenceId = User.FindFirst("ReferenceID")?.Value;

            if (string.IsNullOrEmpty(referenceId))
            {
                return Unauthorized(new
                {
                    message = "User identity could not be determined."
                });
            }

            if (!Guid.TryParse(referenceId, out Guid userID))
            {
                return Unauthorized(new
                {
                    message = "Invalid user identity."
                });
            }

            // Only allow valid availability statuses
   var allowedStatuses = new[]
{
    "Online",
    "On Break",
    "Occupied",
    "Offline"
};

            if (!allowedStatuses.Contains(request.AvailabilityStatus))
            {
                return BadRequest(new
                {
                    message = "Invalid availability status.",
                    allowedStatuses
                });
            }

            // Find the logged-in healthcare worker
            var user = await _context.Users.FindAsync(userID);

            if (user == null)
            {
                return NotFound(new
                {
                    message = "Healthcare worker not found."
                });
            }

            // Make sure this account is actually a healthcare worker
            if (user.Position != "Doctor" &&
    user.Position != "Nurse" &&
    user.Position != "Nurse/Midwife")
{
    return BadRequest(new
    {
        message = "Only healthcare workers can change availability."
    });
}

            // Update availability
            user.AvailabilityStatus = request.AvailabilityStatus;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Availability updated successfully.",
                userID = user.UserID,
                availabilityStatus = user.AvailabilityStatus
            });
        }

        
    }
    
}