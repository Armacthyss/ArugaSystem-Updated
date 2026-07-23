using Microsoft.AspNetCore.Mvc;
using System.Data;
using Microsoft.Data.SqlClient;
using Dapper;
using AndroidWebAPI.Models;

namespace AndroidWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChildrenController : ControllerBase
    {
        private readonly string _connectionString;

        public ChildrenController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? "";
        }

        // ── CREATE: POST /api/Children ────────────────────────────
        [HttpPost]
        public async Task<IActionResult> CreateChild([FromBody] CreateChildDto dto)
        {
            try
            {
                // Validation
                if (string.IsNullOrWhiteSpace(dto.FirstName))
                    return BadRequest(new { message = "FirstName is required." });
                if (string.IsNullOrWhiteSpace(dto.LastName))
                    return BadRequest(new { message = "LastName is required." });
                if (dto.BirthDate == default)
                    return BadRequest(new { message = "BirthDate is required." });
                if (dto.Parents == null || !dto.Parents.Any())
                    return BadRequest(new { message = "At least one parent link is required." });

                using IDbConnection connection = new SqlConnection(_connectionString);
                connection.Open();
                using var transaction = connection.BeginTransaction();

                try
                {
                    // Verify all parents exist
                    foreach (var link in dto.Parents)
                    {
                        var parentExists = await connection.QueryFirstOrDefaultAsync<dynamic>(
                            "SELECT ParentID FROM dbo.Parents WHERE ParentID = @ParentID",
                            new { ParentID = link.ParentID },
                            transaction
                        );
                        if (parentExists == null)
                        {
                            transaction.Rollback();
                            return BadRequest(new { message = $"Parent not found: {link.ParentID}" });
                        }
                    }

                    var childID = Guid.NewGuid();

                    string insertChildQuery = @"
                        INSERT INTO dbo.Children
                            (ChildID, FirstName, MiddleName, LastName, BirthDate, PlaceOfBirth, Sex,
                             Barangay, Address, HealthCenter, CreatedAt, UpdatedAt)
                        VALUES
                            (@ChildID, @FirstName, @MiddleName, @LastName, @BirthDate, @PlaceOfBirth, @Sex,
                             @Barangay, @Address, @HealthCenter, SYSUTCDATETIME(), SYSUTCDATETIME())";

                    await connection.ExecuteAsync(insertChildQuery, new
                    {
                        ChildID = childID,
                        FirstName = dto.FirstName,
                        MiddleName = dto.MiddleName ?? string.Empty,
                        LastName = dto.LastName,
                        BirthDate = dto.BirthDate,
                        PlaceOfBirth = dto.PlaceOfBirth ?? string.Empty,
                        Sex = dto.Sex ?? string.Empty,
                        Barangay = dto.Barangay,
                        Address = dto.Address ?? string.Empty,
                        HealthCenter = dto.HealthCenter ?? string.Empty
                    }, transaction);

                    string insertRelationshipQuery = @"
                        INSERT INTO dbo.ChildParentRelationship
                            (RelationshipID, ChildID, ParentID, RelationshipType, IsPrimaryContact,
                             CanReceiveNotifications, Status, CreatedAt, UpdatedAt)
                        VALUES
                            (@RelationshipID, @ChildID, @ParentID, @RelationshipType, @IsPrimaryContact,
                             @CanReceiveNotifications, @Status, SYSUTCDATETIME(), SYSUTCDATETIME())";

                    foreach (var link in dto.Parents)
                    {
                        await connection.ExecuteAsync(insertRelationshipQuery, new
                        {
                            RelationshipID = Guid.NewGuid(),
                            ChildID = childID,
                            ParentID = link.ParentID,
                            RelationshipType = link.RelationshipType ?? "Parent",
                            IsPrimaryContact = link.IsPrimaryContact,
                            CanReceiveNotifications = link.CanReceiveNotifications,
                            Status = "Active"
                        }, transaction);
                    }

                    transaction.Commit();

                    return CreatedAtAction(nameof(GetChildrenByParent), new { parentId = dto.Parents.First().ParentID }, new
                    {
                        childID = childID,
                        firstName = dto.FirstName,
                        middleName = dto.MiddleName,
                        lastName = dto.LastName,
                        birthDate = dto.BirthDate,
                        placeOfBirth = dto.PlaceOfBirth,
                        sex = dto.Sex,
                        barangay = dto.Barangay,
                        address = dto.Address,
                        healthCenter = dto.HealthCenter,
                        parents = dto.Parents.Select(l => new
                        {
                            parentID = l.ParentID,
                            relationshipType = l.RelationshipType,
                            isPrimaryContact = l.IsPrimaryContact,
                            canReceiveNotifications = l.CanReceiveNotifications
                        })
                    });
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred: " + ex.Message });
            }
        }

        // ── UPDATE: PUT /api/Children/{id} ────────────────────────
        // Only updates columns that exist in the (new) Children table.
        // Relationship changes are NOT handled here.
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateChild(Guid id, [FromBody] UpdateChildDto dto)
        {
            try
            {
                using IDbConnection connection = new SqlConnection(_connectionString);

                // Verify child exists
                var existing = await connection.QueryFirstOrDefaultAsync<dynamic>(
                    "SELECT ChildID FROM dbo.Children WHERE ChildID = @ChildID",
                    new { ChildID = id }
                );
                if (existing == null)
                    return NotFound(new { message = "Child not found." });

                // Validate required fields
                if (string.IsNullOrWhiteSpace(dto.FirstName))
                    return BadRequest(new { message = "FirstName is required." });
                if (string.IsNullOrWhiteSpace(dto.LastName))
                    return BadRequest(new { message = "LastName is required." });

                string query = @"
                    UPDATE dbo.Children
                    SET
                        FirstName = @FirstName,
                        MiddleName = @MiddleName,
                        LastName = @LastName,
                        BirthDate = @BirthDate,
                        PlaceOfBirth = @PlaceOfBirth,
                        Sex = @Sex,
                        Barangay = @Barangay,
                        Address = @Address,
                        HealthCenter = @HealthCenter,
                        UpdatedAt = SYSUTCDATETIME()
                    WHERE ChildID = @ChildID";

                await connection.ExecuteAsync(query, new
                {
                    ChildID = id,
                    FirstName = dto.FirstName,
                    MiddleName = dto.MiddleName ?? string.Empty,
                    LastName = dto.LastName,
                    BirthDate = dto.BirthDate,
                    PlaceOfBirth = dto.PlaceOfBirth ?? string.Empty,
                    Sex = dto.Sex ?? string.Empty,
                    Barangay = dto.Barangay,
                    Address = dto.Address ?? string.Empty,
                    HealthCenter = dto.HealthCenter ?? string.Empty
                });

                return Ok(new { message = "Child updated successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred: " + ex.Message });
            }
        }

        // ── READ: GET /api/Children/parent/{parentId} ─────────────
        // Returns children linked to this parent via ChildParentRelationship.
        // Computes "myRelationship" from RelationshipType for that parent.
        [HttpGet("parent/{parentId}")]
        public async Task<IActionResult> GetChildrenByParent(Guid parentId)
        {
            using IDbConnection connection = new SqlConnection(_connectionString);

            var sql = @"
                SELECT
                    c.ChildID,
                    c.FirstName,
                    c.MiddleName,
                    c.LastName,
                    c.BirthDate,
                    c.PlaceOfBirth,
                    c.Sex,
                    c.HealthCenter,
                    c.Barangay,
                    c.Address,
                    r.RelationshipType,
                    r.IsPrimaryContact,
                    r.CanReceiveNotifications
                FROM dbo.Children c
                INNER JOIN dbo.ChildParentRelationship r ON r.ChildID = c.ChildID
                WHERE r.ParentID = @ParentId
                  AND r.Status = 'Active'";

            var rows = await connection.QueryAsync<dynamic>(sql, new { ParentId = parentId });

            var result = rows.Select(row => new
            {
                childID = row.ChildID,
                firstName = row.FirstName,
                middleName = row.MiddleName,
                lastName = row.LastName,
                birthDate = row.BirthDate,
                placeOfBirth = row.PlaceOfBirth,
                sex = row.Sex,
                healthCenter = row.HealthCenter,
                barangay = row.Barangay,
                address = row.Address,

                // Computed: which role does the logged-in parent have for this child
                myRelationship = row.RelationshipType,
                isPrimaryContact = row.IsPrimaryContact,
                canReceiveNotifications = row.CanReceiveNotifications
            });

            return Ok(result);
        }

        // ── READ: GET /api/Children/all ───────────────────────────
        // Returns every child with all linked parents (supports multiple).
        [HttpGet("all")]
        public async Task<IActionResult> GetAllChildren()
        {
            using IDbConnection connection = new SqlConnection(_connectionString);

            var sql = @"
                SELECT
                    c.ChildID,
                    c.FirstName,
                    c.MiddleName,
                    c.LastName,
                    c.BirthDate,
                    c.PlaceOfBirth,
                    c.Sex,
                    c.HealthCenter,
                    c.Barangay,
                    c.Address,
                    r.ParentID,
                    r.RelationshipType,
                    r.IsPrimaryContact,
                    r.CanReceiveNotifications,
                    p.FirstName AS ParentFirstName,
                    p.LastName  AS ParentLastName
                FROM dbo.Children c
                LEFT JOIN dbo.ChildParentRelationship r ON r.ChildID = c.ChildID AND r.Status = 'Active'
                LEFT JOIN dbo.Parents p ON p.ParentID = r.ParentID
                ORDER BY c.ChildID";

            var rows = await connection.QueryAsync<dynamic>(sql);

            // Group by child so each child has a list of linked parents
            var grouped = rows
                .GroupBy(row => (Guid)row.ChildID)
                .Select(g =>
                {
                    var first = g.First();

                    var parents = g
                        .Where(row => row.ParentID != null)
                        .Select(row => new
                        {
                            parentID = row.ParentID,
                            parentName = (row.ParentFirstName != null && row.ParentLastName != null)
                                ? $"{row.ParentFirstName} {row.ParentLastName}"
                                : "—",
                            relationshipType = row.RelationshipType,
                            isPrimaryContact = row.IsPrimaryContact,
                            canReceiveNotifications = row.CanReceiveNotifications
                        })
                        .ToList();

                    // Convenience: first/primary parent name for simple Vue table display
                    var primaryParent = parents.FirstOrDefault(p => p.isPrimaryContact == true) ?? parents.FirstOrDefault();

                    return new
                    {
                        childID = first.ChildID,
                        firstName = first.FirstName,
                        middleName = first.MiddleName,
                        lastName = first.LastName,
                        birthDate = first.BirthDate,
                        placeOfBirth = first.PlaceOfBirth,
                        sex = first.Sex,
                        healthCenter = first.HealthCenter,
                        barangay = first.Barangay,
                        address = first.Address,

                        // Backwards-compatible convenience fields for existing Vue table bindings
                        parentID = primaryParent?.parentID,
                        parentName = primaryParent?.parentName ?? "—",

                        // Full list of linked parents (supports multiple)
                        parents = parents
                    };
                });

            return Ok(grouped);
        }
    }

    // ── DTOs ──────────────────────────────────────────────────────
    public class ParentLinkDto
    {
        public Guid ParentID { get; set; }
        public string? RelationshipType { get; set; } // e.g. "Mother", "Father", "Guardian"
        public bool IsPrimaryContact { get; set; }
        public bool CanReceiveNotifications { get; set; } = true;
    }

    public class CreateChildDto
    {
        public string FirstName { get; set; } = string.Empty;
        public string? MiddleName { get; set; }
        public string LastName { get; set; } = string.Empty;
        public DateTime BirthDate { get; set; }
        public string? PlaceOfBirth { get; set; }
        public string? Sex { get; set; }
        public int? Barangay { get; set; }
        public string? Address { get; set; }
        public string? HealthCenter { get; set; }

        // One or more parent links to create in ChildParentRelationship
        public List<ParentLinkDto> Parents { get; set; } = new();
    }

    public class UpdateChildDto
    {
        public string FirstName { get; set; } = string.Empty;
        public string? MiddleName { get; set; }
        public string LastName { get; set; } = string.Empty;
        public DateTime BirthDate { get; set; }
        public string? PlaceOfBirth { get; set; }
        public string? Sex { get; set; }
        public int? Barangay { get; set; }
        public string? Address { get; set; }
        public string? HealthCenter { get; set; }

        // No relationship fields here — relationship updates are handled
        // by a separate endpoint/controller against ChildParentRelationship.
    }
}