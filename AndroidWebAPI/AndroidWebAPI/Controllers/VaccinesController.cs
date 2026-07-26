using AndroidWebAPI.Data;
using AndroidWebAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace AndroidWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VaccinesController : ControllerBase
    {
        private readonly VaccineRepository _repository;

        public VaccinesController(VaccineRepository repository)
        {
            _repository = repository;
        }

        // ===========================================
        // GET ALL
        // GET: api/Vaccines
        // ===========================================
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var vaccines = await _repository.GetAllAsync();
            return Ok(vaccines);
        }

        // ===========================================
        // GET BY ID
        // GET: api/Vaccines/{id}
        // ===========================================
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var vaccine = await _repository.GetByIdAsync(id);

            if (vaccine == null)
                return NotFound();

            return Ok(vaccine);
        }

        // ===========================================
        // CREATE
        // POST: api/Vaccines
        // ===========================================
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Vaccine vaccine)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var created = await _repository.CreateAsync(vaccine);

            return CreatedAtAction(
                nameof(Get),
                new { id = created.VaccineID },
                created);
        }

        // ===========================================
        // UPDATE
        // PUT: api/Vaccines/{id}
        // ===========================================
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Vaccine vaccine)
        {
            if (id != vaccine.VaccineID)
                return BadRequest("ID mismatch.");

            var existing = await _repository.GetByIdAsync(id);

            if (existing == null)
                return NotFound();

            var updated = await _repository.UpdateAsync(vaccine);

            return Ok(updated);
        }

        // ===========================================
        // DELETE
        // DELETE: api/Vaccines/{id}
        // ===========================================
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _repository.DeleteAsync(id);

            if (!success)
                return NotFound();

            return Ok(new
            {
                message = "Vaccine deleted successfully."
            });
        }
    }
}