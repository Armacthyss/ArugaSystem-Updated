using Microsoft.AspNetCore.Mvc;
using AndroidWebAPI.Data;
using AndroidWebAPI.Models;

namespace AndroidWebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VaccinationScheduleRulesController : ControllerBase
    {
        private readonly IVaccinationScheduleRuleRepository _repository;

        public VaccinationScheduleRulesController(IVaccinationScheduleRuleRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _repository.GetAllAsync());
        }

        [HttpGet("vaccine/{vaccineId}")]
        public async Task<IActionResult> GetByVaccine(int vaccineId)
        {
            return Ok(await _repository.GetByVaccineAsync(vaccineId));
        }

        [HttpGet("{vaccineId}/{doseNumber}")]
        public async Task<IActionResult> GetRule(int vaccineId, int doseNumber)
        {
            var rule = await _repository.GetRuleAsync(vaccineId, doseNumber);

            if (rule == null)
                return NotFound();

            return Ok(rule);
        }

        [HttpPost]
        public async Task<IActionResult> Create(VaccinationScheduleRule rule)
        {
            await _repository.AddAsync(rule);

            return CreatedAtAction(nameof(GetRule),
                new { vaccineId = rule.VaccineID, doseNumber = rule.DoseNumber }, rule);
        }

        [HttpPut]
        public async Task<IActionResult> Update(VaccinationScheduleRule rule)
        {
            await _repository.UpdateAsync(rule);
            return NoContent();
        }

        [HttpDelete("{ruleId}")]
        public async Task<IActionResult> Delete(int ruleId)
        {
            await _repository.DeleteAsync(ruleId);
            return NoContent();
        }
    }
}