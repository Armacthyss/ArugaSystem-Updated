using Microsoft.AspNetCore.Mvc;
using AndroidWebAPI.Data;
using AndroidWebAPI.Models;

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
public async Task<IActionResult> RecordVaccination([FromBody] VaccinationRecord record)
{
    await _repository.RecordVaccinationAsync(record);

    return Ok(new
    {
        message = "Vaccination recorded successfully."
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
    }
}