// ===================================
// ADD THESE METHODS TO ParentsController.cs
// ===================================

// GET /api/Parents/all
// Returns all parents (for staff dashboard)
[HttpGet("all")]
public async Task<IActionResult> GetAllParents()
{
    var parents = await _parentRepo.GetAllAsync();
    return Ok(parents);
}

// GET /api/Parents/{id}
// Get single parent by ID
[HttpGet("{id}")]
public async Task<IActionResult> GetParentById(Guid id)
{
    var parent = await _parentRepo.GetByIdAsync(id);
    if (parent == null)
        return NotFound(new { message = "Parent not found" });
    return Ok(parent);
}

// POST /api/Parents
// Create new parent account
[HttpPost]
public async Task<IActionResult> CreateParent([FromBody] CreateParentDto dto)
{
    if (string.IsNullOrEmpty(dto.FirstName) || string.IsNullOrEmpty(dto.LastName) || string.IsNullOrEmpty(dto.Email))
        return BadRequest(new { message = "FirstName, LastName, and Email are required" });

    var newParent = new Parent
    {
        ParentID = Guid.NewGuid(),
        FirstName = dto.FirstName,
        LastName = dto.LastName,
        MiddleName = dto.MiddleName ?? string.Empty,
        Email = dto.Email,
        ContactNo = dto.ContactNo,
        Address = dto.Address ?? string.Empty,
        BarangayNo = dto.BarangayNo ?? string.Empty,
        Password = dto.Password, // TODO: Hash this with BCrypt!
    };

    await _parentRepo.CreateAsync(newParent);
    return Ok(newParent);
}

// PUT /api/Parents/{id}
// Update parent information
[HttpPut("{id}")]
public async Task<IActionResult> UpdateParent(Guid id, [FromBody] UpdateParentDto dto)
{
    var parent = await _parentRepo.GetByIdAsync(id);
    if (parent == null)
        return NotFound(new { message = "Parent not found" });

    parent.FirstName = dto.FirstName ?? parent.FirstName;
    parent.LastName = dto.LastName ?? parent.LastName;
    parent.Email = dto.Email ?? parent.Email;
    parent.ContactNo = dto.ContactNo ?? parent.ContactNo;
    parent.Address = dto.Address ?? parent.Address;
    parent.BarangayNo = dto.BarangayNo ?? parent.BarangayNo;

    await _parentRepo.UpdateAsync(parent);
    return Ok(parent);
}

// ===== DTOs for Parents =====
public class CreateParentDto
{
    public string FirstName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string ContactNo { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? BarangayNo { get; set; }
    public string Password { get; set; } = string.Empty;
}

public class UpdateParentDto
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? ContactNo { get; set; }
    public string? Address { get; set; }
    public string? BarangayNo { get; set; }
}


// ===================================
// ADD THESE METHODS TO ChildrenController.cs
// ===================================

// POST /api/Children
// Create new child record
[HttpPost]
public async Task<IActionResult> CreateChild([FromBody] CreateChildDto dto)
{
    if (string.IsNullOrEmpty(dto.FirstName) || string.IsNullOrEmpty(dto.LastName))
        return BadRequest(new { message = "FirstName and LastName are required" });

    var newChild = new Child
    {
        ChildID = Guid.NewGuid(),
        FirstName = dto.FirstName,
        LastName = dto.LastName,
        MiddleName = dto.MiddleName ?? string.Empty,
        BirthDate = dto.BirthDate,
        PlaceOfBirth = dto.PlaceOfBirth,
        Sex = dto.Sex,
        Barangay = dto.Barangay ?? 0,
        Address = dto.Address,
        ParentID = dto.ParentId,
        MotherID = dto.MotherId,
        FatherID = dto.FatherId,
        GuardianID = dto.GuardianId,
    };

    using IDbConnection connection = new SqlConnection(_connectionString);
    var sql = @"
        INSERT INTO dbo.Children 
        (ChildID, FirstName, MiddleName, LastName, BirthDate, PlaceOfBirth, Sex, Barangay, Address, ParentID, MotherID, FatherID, GuardianID)
        VALUES 
        (@ChildId, @FirstName, @MiddleName, @LastName, @BirthDate, @PlaceOfBirth, @Sex, @Barangay, @Address, @ParentId, @MotherId, @FatherId, @GuardianId)";

    await connection.ExecuteAsync(sql, newChild);
    return Ok(newChild);
}

// PUT /api/Children/{id}
// Update child record
[HttpPut("{id}")]
public async Task<IActionResult> UpdateChild(Guid id, [FromBody] UpdateChildDto dto)
{
    var sql = @"
        UPDATE dbo.Children SET
            FirstName = @FirstName,
            MiddleName = @MiddleName,
            LastName = @LastName,
            BirthDate = @BirthDate,
            PlaceOfBirth = @PlaceOfBirth,
            Sex = @Sex,
            Barangay = @Barangay,
            Address = @Address
        WHERE ChildID = @ChildId";

    using IDbConnection connection = new SqlConnection(_connectionString);
    var rowsAffected = await connection.ExecuteAsync(sql, new
    {
        ChildId = id,
        dto.FirstName,
        dto.MiddleName,
        dto.LastName,
        dto.BirthDate,
        dto.PlaceOfBirth,
        dto.Sex,
        dto.Barangay,
        dto.Address
    });

    if (rowsAffected == 0)
        return NotFound(new { message = "Child not found" });

    return Ok(new { message = "Child updated successfully" });
}

// DELETE /api/Children/{childId}/unlink-parent/{parentId}
// Remove parent link from child
[HttpDelete("{childId}/unlink-parent/{parentId}")]
public async Task<IActionResult> UnlinkParentFromChild(Guid childId, Guid parentId)
{
    var sql = @"
        UPDATE dbo.Children SET
            ParentID = NULL,
            MotherID = CASE WHEN MotherID = @ParentId THEN NULL ELSE MotherID END,
            FatherID = CASE WHEN FatherID = @ParentId THEN NULL ELSE FatherID END,
            GuardianID = CASE WHEN GuardianID = @ParentId THEN NULL ELSE GuardianID END
        WHERE ChildID = @ChildId";

    using IDbConnection connection = new SqlConnection(_connectionString);
    var rowsAffected = await connection.ExecuteAsync(sql, new { ChildId = childId, ParentId = parentId });

    if (rowsAffected == 0)
        return NotFound(new { message = "Child not found" });

    return Ok(new { message = "Parent unlinked successfully" });
}

// ===== DTOs for Children =====
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
    public Guid? ParentId { get; set; }
    public Guid? MotherId { get; set; }
    public Guid? FatherId { get; set; }
    public Guid? GuardianId { get; set; }
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
}
