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
    }
}