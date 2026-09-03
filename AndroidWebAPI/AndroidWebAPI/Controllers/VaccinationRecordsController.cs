using Microsoft.AspNetCore.Mvc;
using AndroidWebAPI.Data;
using AndroidWebAPI.Models;
using AndroidWebAPI.DTOs;

namespace AndroidWebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VaccinationRecordsController : ControllerBase
    {
        private readonly IVaccinationRecordRepository _repository;

        public VaccinationRecordsController(IVaccinationRecordRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _repository.GetAllAsync());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var record = await _repository.GetByIdAsync(id);

            if (record == null)
                return NotFound();

            return Ok(record);
        }

        [HttpGet("child/{childId}")]
        public async Task<IActionResult> GetByChild(Guid childId)
        {
            return Ok(await _repository.GetByChildAsync(childId));
        }

[HttpPost]
public async Task<IActionResult> RecordVaccination(
    [FromBody] RecordVaccinationDto dto)
{
    var record = new VaccinationRecord
    {
        ChildID = dto.ChildID,
        VaccineID = dto.VaccineID,
        DoseNumber = dto.DoseNumber,
        VaccinationDate = dto.VaccinationDate,
        InventoryID = dto.InventoryID,
        AdministeredByPersonnelID = dto.AdministeredByPersonnelID,
        NurseObservation = dto.NurseObservation,
        DoctorDiagnosis = dto.DoctorDiagnosis,
        DoctorDiagnosedByPersonnelID = dto.DoctorDiagnosedByPersonnelID,
        DoctorDiagnosedAt = dto.DoctorDiagnosedAt
    };

    await _repository.RecordVaccinationAsync(record);

    return Ok(new
    {
        message = "Vaccination recorded successfully.",
        vaccinationRecordID = record.VaccinationRecordID,
        recordCode = record.RecordCode
    });
}

        [HttpPut]
        public async Task<IActionResult> Update(VaccinationRecord record)
        {
            await _repository.UpdateAsync(record);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _repository.DeleteAsync(id);
            return NoContent();
        }

        [HttpPost("historical")]
public async Task<IActionResult> RecordHistoricalVaccinations(
    [FromBody] HistoricalVaccinationSubmissionDto submission)
{
    await _repository.RecordHistoricalVaccinationsAsync(submission);

    return Ok(new
    {
        message = "Historical vaccination records saved successfully."
    });
}
    

    // GET: api/VaccinationRecords/stats
[HttpGet("stats")]
public async Task<IActionResult> GetStats()
{
    var today = DateTime.Today;
    var tomorrow = today.AddDays(1);
    var weekStart = today.AddDays(-(int)today.DayOfWeek);

    var allRecords = await _repository.GetAllAsync();

    var vaccinatedToday = allRecords.Count(r =>
        r.Status == "Completed" &&
        r.VaccinationDate >= today &&
        r.VaccinationDate < tomorrow);

    var pendingToday = allRecords.Count(r =>
        r.Status == "Pending" &&
        r.VaccinationDate >= today &&
        r.VaccinationDate < tomorrow);

    var weeklyTotal = allRecords.Count(r =>
        r.Status == "Completed" &&
        r.VaccinationDate >= weekStart &&
        r.VaccinationDate < tomorrow);

    var missedTotal = allRecords.Count(r =>
        r.Status == "Deferred" ||
        r.Status == "Cancelled");

    return Ok(new
    {
        vaccinatedToday,
        pendingToday,
        missedTotal,
        weeklyTotal
    });
}


// GET: api/VaccinationRecords/pending-today
[HttpGet("pending-today")]
public async Task<IActionResult> GetPendingToday()
{
    var today = DateTime.Today;
    var tomorrow = today.AddDays(1);

    var records = (await _repository.GetAllAsync())
        .Where(r =>
            r.Status == "Pending" &&
            r.VaccinationDate >= today &&
            r.VaccinationDate < tomorrow)
        .Select(r => new
        {
            recordId = r.VaccinationRecordID,
            childId = r.ChildID,

            childName = r.Child != null
                ? $"{r.Child.FirstName} {r.Child.LastName}".Trim()
                : "Unknown",

            vaccineId = r.VaccineID,

            vaccineName = r.Vaccine != null
                ? r.Vaccine.VaccineName
                : "Unknown",

            doseNumber = r.DoseNumber,
            vaccinationDate = r.VaccinationDate,
            status = r.Status
        })
        .ToList();

    return Ok(records);
}


// GET: api/VaccinationRecords/completed-today
[HttpGet("completed-today")]
public async Task<IActionResult> GetCompletedToday()
{
    var today = DateTime.Today;
    var tomorrow = today.AddDays(1);

    var records = (await _repository.GetAllAsync())
        .Where(r =>
            r.Status == "Completed" &&
            r.VaccinationDate >= today &&
            r.VaccinationDate < tomorrow)
        .OrderByDescending(r => r.VaccinationDate)
        .Select(r => new
        {
            recordId = r.VaccinationRecordID,
            childId = r.ChildID,

            childName = r.Child != null
                ? $"{r.Child.FirstName} {r.Child.LastName}".Trim()
                : "Unknown",

            vaccineName = r.Vaccine != null
                ? r.Vaccine.VaccineName
                : "Unknown",

            doseNumber = r.DoseNumber,
            vaccinationDate = r.VaccinationDate
        })
        .ToList();

    return Ok(records);
}}}
