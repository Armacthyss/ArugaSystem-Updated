using AndroidWebAPI.Data;
using Microsoft.AspNetCore.Mvc;
using AndroidWebAPI.DTOs;

namespace AndroidWebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VaccinationTimelineController : ControllerBase
    {
        private readonly IVaccinationTimelineRepository _repository;

        public VaccinationTimelineController(IVaccinationTimelineRepository repository)
        {
            _repository = repository;
        }

        // GET: api/VaccinationTimeline
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var timelines = await _repository.GetAllAsync();
            return Ok(timelines);
        }

        // GET: api/VaccinationTimeline/{timelineId}
        [HttpGet("{timelineId}")]
        public async Task<IActionResult> GetById(Guid timelineId)
        {
            var timeline = await _repository.GetByIdAsync(timelineId);

            if (timeline == null)
                return NotFound();

            return Ok(timeline);
        }

        // GET: api/VaccinationTimeline/child/{childId}
        [HttpGet("child/{childId}")]
        public async Task<IActionResult> GetByChild(Guid childId)
        {
            var timelines = await _repository.GetByChildAsync(childId);
            return Ok(timelines);
        }

        // POST: api/VaccinationTimeline/generate/{childId}
        [HttpPost("generate/{childId}")]
        public async Task<IActionResult> Generate(Guid childId)
        {
            await _repository.GenerateTimelineAsync(childId);

            return Ok(new
            {
                message = "Vaccination timeline generated successfully."
            });
        }

        // PUT: api/VaccinationTimeline/complete/{timelineId}
        [HttpPut("complete/{timelineId}")]
        public async Task<IActionResult> MarkCompleted(Guid timelineId)
        {
            await _repository.MarkCompletedAsync(timelineId);

            return Ok(new
            {
                message = "Vaccination marked as completed."
            });
        }

        // POST: api/VaccinationTimeline/regenerate/{childId}
        [HttpPost("regenerate/{childId}")]
        public async Task<IActionResult> Regenerate(Guid childId)
        {
            await _repository.RegenerateTimelineAsync(childId);

            return Ok(new
            {
                message = "Vaccination timeline regenerated."
            });
        }

        // GET: api/VaccinationTimeline/due-today
        [HttpGet("due-today")]
        public async Task<IActionResult> GetDueToday()
        {
            var timelines = await _repository.GetDueTodayAsync();
            return Ok(timelines);
        }

        // GET: api/VaccinationTimeline/upcoming/{days}
        [HttpGet("upcoming/{days}")]
        public async Task<IActionResult> GetUpcoming(int days)
        {
            var timelines = await _repository.GetUpcomingAsync(days);
            return Ok(timelines);
        }
   [HttpGet("summary/{childId}")]
public async Task<IActionResult> GetSummary(Guid childId)
{
    var summary = await _repository.GetTimelineSummaryAsync(childId);
    return Ok(summary);
}
    }
}