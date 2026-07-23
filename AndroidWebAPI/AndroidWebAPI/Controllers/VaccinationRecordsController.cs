// Controllers/VaccinationRecordsController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AndroidWebAPI.Data;
using AndroidWebAPI.Models;

[ApiController]
[Route("api/[controller]")]
public class VaccinationRecordsController : ControllerBase
{
    private readonly AppDbContext _context;
    public VaccinationRecordsController(AppDbContext context) => _context = context;

    // ── PARENT DASHBOARD ENDPOINTS ────────────────────────────────────────

    // GET api/VaccinationRecords/child/{childId}
    [HttpGet("child/{childId}")]
    public async Task<IActionResult> GetByChild(Guid childId)
    {
        var rawRecords = await _context.VaccinationRecords
            .Where(r => r.ChildID == childId)
            .ToListAsync();

        var vaccines = await _context.Vaccines
            .Select(v => new { v.VaccineID, v.VaccineName })
            .ToListAsync();

        var records = rawRecords
            .Join(vaccines,
                  r => r.VaccineID,
                  v => v.VaccineID,
                  (r, v) => new
                  {
                      recordID           = r.RecordID,
                      childID            = r.ChildID,
                      vaccineID          = r.VaccineID,
                      vaccineName        = v.VaccineName,
                      doseNumber         = r.DoseNumber,
                      dateAdministered   = r.DateAdministered,
                      administeredByName = r.AdministeredByName,
                      lotNumber          = r.LotNumber,
                      status             = r.Status,
                      scheduledDate      = r.ScheduledDate,
                      remarks            = r.Remarks
                  })
            .OrderBy(r => r.dateAdministered)
            .ToList();

        return Ok(records);
    }

    // GET api/VaccinationRecords/stats/{childId}
    [HttpGet("stats/{childId}")]
    public async Task<IActionResult> GetStats(Guid childId)
    {
        var records = await _context.VaccinationRecords
            .Where(r => r.ChildID == childId)
            .ToListAsync();

        return Ok(new
        {
            completed = records.Count(r => r.Status == "Completed"),
            scheduled = records.Count(r => r.Status == "Pending"),
            overdue   = records.Count(r => r.Status == "Missed"),
        });
    }

    // ── DOCTOR RECORDS PAGE ENDPOINT ──────────────────────────────────────
[HttpGet("all")]
public async Task<IActionResult> GetAllRecords()
{
    var records = await (
        from vr in _context.VaccinationRecords
        join c in _context.Children on vr.ChildID equals c.ChildID
        join v in _context.Vaccines on vr.VaccineID equals v.VaccineID
        join r in _context.Set<ChildParentRelationship>() on c.ChildID equals r.ChildID into rel
        from relationship in rel.DefaultIfEmpty()
        join p in _context.Parents on relationship.ParentID equals p.ParentID into parents
        from parent in parents.DefaultIfEmpty()
        select new
        {
            recordID = vr.RecordID,
            childID = c.ChildID,
            childName = c.FirstName + " " + c.LastName,
            parentName = parent != null
                ? parent.FirstName + " " + parent.LastName
                : "No Parent",

            vaccineID = v.VaccineID,
            vaccineName = v.VaccineName,

            doseNumber = vr.DoseNumber,
            dateAdministered = vr.DateAdministered,
            scheduledDate = vr.ScheduledDate,
            lotNumber = vr.LotNumber,
            status = vr.Status,
            administeredBy = vr.AdministeredBy,
            administeredByName = vr.AdministeredByName,
            remarks = vr.Remarks
        })
        .ToListAsync();

    return Ok(records.OrderByDescending(r => r.dateAdministered ?? r.scheduledDate));
}

    // ── CALENDAR ENDPOINT ─────────────────────────────────────────────────

    // GET api/VaccinationRecords/all-completed
    [HttpGet("all-completed")]
    public async Task<IActionResult> GetAllCompleted()
    {
        var rawRecords = await _context.VaccinationRecords
            .Where(r => r.Status == "Completed" && r.DateAdministered != null)
            .ToListAsync();

        var childIds = rawRecords.Select(r => r.ChildID).Distinct().ToList();
        var children = await _context.Children
            .Where(c => childIds.Contains(c.ChildID))
            .ToListAsync();

        var vaccines = await _context.Vaccines
            .Select(v => new { v.VaccineID, v.VaccineName })
            .ToListAsync();

        var result = rawRecords
            .Join(vaccines, r => r.VaccineID, v => v.VaccineID, (r, v) => new { r, vaccineName = v.VaccineName })
            .Select(x =>
            {
                var child = children.FirstOrDefault(c => c.ChildID == x.r.ChildID);
                return new
                {
                    recordID           = x.r.RecordID,
                    childID            = x.r.ChildID,
                    childName          = child != null ? $"{child.FirstName} {child.LastName}" : "Unknown",
                    vaccineID          = x.r.VaccineID,
                    vaccineName        = x.vaccineName,
                    doseNumber         = x.r.DoseNumber,
                    dateAdministered   = x.r.DateAdministered,
                    administeredByName = x.r.AdministeredByName,
                    lotNumber          = x.r.LotNumber,
                    remarks            = x.r.Remarks,
                    status             = x.r.Status,
                };
            })
            .OrderByDescending(r => r.dateAdministered)
            .ToList();

        return Ok(result);
    }

    // ── DOCTOR DASHBOARD ENDPOINTS ────────────────────────────────────────

    // GET api/VaccinationRecords/pending-today
    [HttpGet("pending-today")]
public async Task<IActionResult> GetPendingToday()
{
    var todayStart = DateTime.Today;
    var todayEnd = todayStart.AddDays(1);

    var records = await (
        from vr in _context.VaccinationRecords
        join c in _context.Children on vr.ChildID equals c.ChildID
        join v in _context.Vaccines on vr.VaccineID equals v.VaccineID
        join r in _context.Set<ChildParentRelationship>()
            on c.ChildID equals r.ChildID into rel
        from relationship in rel.DefaultIfEmpty()
        join p in _context.Parents
            on relationship.ParentID equals p.ParentID into parentJoin
        from parent in parentJoin.DefaultIfEmpty()
        where vr.ScheduledDate >= todayStart
              && vr.ScheduledDate < todayEnd
              && vr.Status == "Pending"
        select new
        {
            recordId = vr.RecordID,
            childId = c.ChildID,
            childName = c.FirstName + " " + c.LastName,
            parentName = parent != null
                ? parent.FirstName + " " + parent.LastName
                : "Unknown",
            vaccineName = v.VaccineName,
            doseNumber = vr.DoseNumber
        })
        .ToListAsync();

    return Ok(records);
}

    // GET api/VaccinationRecords/completed-today
    [HttpGet("completed-today")]
    public async Task<IActionResult> GetCompletedToday()
    {
        var todayStart = DateTime.Today;
        var todayEnd   = DateTime.Today.AddDays(1);

        var rawRecords = await _context.VaccinationRecords
            .Where(vr => vr.DateAdministered >= todayStart
                      && vr.DateAdministered <  todayEnd
                      && vr.Status == "Completed")
            .ToListAsync();

        var childIds = rawRecords.Select(r => r.ChildID).Distinct().ToList();
        var children = await _context.Children
            .Where(c => childIds.Contains(c.ChildID))
            .ToListAsync();

        var vaccines = await _context.Vaccines
            .Select(v => new { v.VaccineID, v.VaccineName })
            .ToListAsync();

        var result = rawRecords
            .Join(vaccines, r => r.VaccineID, v => v.VaccineID, (r, v) => new { r, vaccineName = v.VaccineName })
            .Select(x =>
            {
                var child = children.FirstOrDefault(c => c.ChildID == x.r.ChildID);
                return new
                {
                    recordId    = x.r.RecordID,
                    childName   = child != null ? $"{child.FirstName} {child.LastName}" : "Unknown",
                    vaccineName = x.vaccineName,
                    doseNumber  = x.r.DoseNumber,
                };
            })
            .ToList();

        return Ok(result);
    }

    // GET api/VaccinationRecords/stats
    [HttpGet("stats")]
    public async Task<IActionResult> GetDashboardStats()
    {
        var todayStart = DateTime.Today;
        var todayEnd   = DateTime.Today.AddDays(1);
        var weekAgo    = DateTime.Today.AddDays(-7);

        return Ok(new
        {
            vaccinatedToday = await _context.VaccinationRecords
                .CountAsync(r => r.DateAdministered >= todayStart
                              && r.DateAdministered <  todayEnd
                              && r.Status == "Completed"),

            pendingToday = await _context.VaccinationRecords
                .CountAsync(r => r.ScheduledDate >= todayStart
                              && r.ScheduledDate <  todayEnd
                              && r.Status == "Pending"),

            missedTotal = await _context.VaccinationRecords
                .CountAsync(r => r.ScheduledDate < todayStart
                              && r.Status == "Pending"),

            weeklyTotal = await _context.VaccinationRecords
                .CountAsync(r => r.DateAdministered >= weekAgo
                              && r.Status == "Completed"),
        });
    }

    // PATCH api/VaccinationRecords/{recordId}
    [HttpPatch("{recordId}")]
    public async Task<IActionResult> MarkAsVaccinated(Guid recordId, [FromBody] VaccinateDto dto)
    {
        var record = await _context.VaccinationRecords.FindAsync(recordId);
        if (record == null) return NotFound();

        record.Status             = "Completed";
        record.DateAdministered   = DateTime.Parse(dto.DateAdministered);
        record.LotNumber          = dto.LotNumber;
        record.Remarks            = dto.Remarks ?? "No adverse reaction";
        record.AdministeredBy = dto.AdministeredBy;
        record.AdministeredByName = dto.AdministeredByName;

        await _context.SaveChangesAsync();
        return Ok(record);
    }

    // POST api/VaccinationRecords
    // POST api/VaccinationRecords
[HttpPost]
public async Task<IActionResult> CreateRecord([FromBody] CreateVaccinationDto dto)
{
    // AdministeredBy is already Guid? — no TryParse needed
    if (dto.AdministeredBy == null || dto.AdministeredBy == Guid.Empty)
        return BadRequest(new { message = "AdministeredBy is required." });

    // Duplicate guard
    var exists = await _context.VaccinationRecords.AnyAsync(r =>
        r.ChildID    == dto.ChildID    &&
        r.VaccineID  == dto.VaccineID  &&
        r.DoseNumber == dto.DoseNumber &&
        r.Status     == "Completed");

    if (exists)
        return Conflict(new { message = "This dose has already been recorded as completed for this child." });

    var record = new VaccinationRecord
    {
        RecordID           = Guid.NewGuid(),
        ChildID            = dto.ChildID,
        VaccineID          = dto.VaccineID,
        DoseNumber         = dto.DoseNumber,
        Status             = "Completed",
        DateAdministered   = DateTime.Parse(dto.DateAdministered),
        LotNumber          = dto.LotNumber,
        Remarks            = dto.Remarks ?? "No adverse reaction",
        AdministeredBy     = dto.AdministeredBy,
        AdministeredByName = dto.AdministeredByName,
        ScheduledDate      = null,
    };

    _context.VaccinationRecords.Add(record);
    await _context.SaveChangesAsync();
    return Ok(record);
}
}

// ── DTOs ──────────────────────────────────────────────────────────────────

public class VaccinateDto
{
    public string  DateAdministered   { get; set; } = string.Empty;
    public string  LotNumber          { get; set; } = string.Empty;
    public string? Remarks            { get; set; }
    public Guid    AdministeredBy     { get; set; }
    public string  AdministeredByName { get; set; } = string.Empty;
    public string  Status             { get; set; } = "Completed";
}

public class CreateVaccinationDto
{
    public Guid    ChildID            { get; set; }
    public int     VaccineID          { get; set; }
    public int     DoseNumber         { get; set; }
    public string  DateAdministered   { get; set; } = string.Empty;
    public string  LotNumber          { get; set; } = string.Empty;
    public string? Remarks            { get; set; }
    public Guid    AdministeredBy     { get; set; }
    public string  AdministeredByName { get; set; } = string.Empty;
}