using AndroidWebAPI.Models;
using AndroidWebAPI.Repositories;
using Microsoft.AspNetCore.Mvc;
using AndroidWebAPI.DTOs;

namespace AndroidWebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QueueController : ControllerBase
    {
        private readonly IQueueRepository _queueRepository;

        public QueueController(IQueueRepository queueRepository)
        {
            _queueRepository = queueRepository;
        }

       // GET: api/Queue
[HttpGet]
public async Task<IActionResult> GetAll()
{
    var queues = await _queueRepository.GetAllAsync();

    var result = queues.Select(q => new QueueResponseDto
    {
        QueueID = q.QueueID,
        QueueNumber = q.QueueNumber,
        BarangayNo = q.Parent?.BarangayNo,
        RequestBy = $"{q.Parent?.FirstName} {q.Parent?.LastName}".Trim(),
        Status = q.Status,
        QueueDate = q.QueueDate,

        Children = q.QueueChildren
            .Select(qc => new QueueChildResponseDto
            {
                ChildID = qc.ChildID,
                Name = $"{qc.Child?.FirstName} {qc.Child?.LastName}".Trim()
            })
            .ToList()
    }).ToList();

    return Ok(result);
}

        // GET: api/Queue/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var queue = await _queueRepository.GetByIdAsync(id);

            if (queue == null)
                return NotFound();

            var result = new QueueResponseDto
{
    QueueID = queue.QueueID,
    QueueNumber = queue.QueueNumber,
    BarangayNo = queue.Parent?.BarangayNo,
    RequestBy = $"{queue.Parent?.FirstName} {queue.Parent?.LastName}".Trim(),
    Status = queue.Status,
    QueueDate = queue.QueueDate,

    Children = queue.QueueChildren
        .Select(qc => new QueueChildResponseDto
        {
            ChildID = qc.ChildID,
            Name = $"{qc.Child?.FirstName} {qc.Child?.LastName}".Trim()
        })
        .ToList()
};

return Ok(result);
        }

        // GET: api/Queue/parent/{parentId}
        [HttpGet("parent/{parentId}")]
        public async Task<IActionResult> GetParentQueue(Guid parentId)
        {
            var queue = await _queueRepository.GetParentQueueAsync(
                parentId,
                DateTime.Today
            );

            if (queue == null)
                return NotFound(new
                {
                    message = "Parent does not have a queue entry for today."
                });

            var result = new QueueResponseDto
{
    QueueID = queue.QueueID,
    QueueNumber = queue.QueueNumber,
    BarangayNo = queue.Parent?.BarangayNo,
    RequestBy = $"{queue.Parent?.FirstName} {queue.Parent?.LastName}".Trim(),
    Status = queue.Status,
    QueueDate = queue.QueueDate,

    Children = queue.QueueChildren
        .Select(qc => new QueueChildResponseDto
        {
            ChildID = qc.ChildID,
            Name = $"{qc.Child?.FirstName} {qc.Child?.LastName}".Trim()
        })
        .ToList()
};

return Ok(result);
        }

        // POST: api/Queue
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateQueueRequest request)
        {
            if (request.ChildIDs == null || request.ChildIDs.Count == 0)
            {
                return BadRequest(new
                {
                    message = "At least one child must be selected."
                });
            }

            var today = DateTime.Today;

            // Prevent duplicate queue entry for the same parent today
            var existingQueue =
                await _queueRepository.GetParentQueueAsync(
                    request.ParentID,
                    today
                );

            if (existingQueue != null)
            {
                return Conflict(new
                {
                    message = "This parent already has a queue entry for today.",
                    queueID = existingQueue.QueueID,
                    queueNumber = existingQueue.QueueNumber
                });
            }

            var nextQueueNumber =
                await _queueRepository.GetNextQueueNumberAsync(today);

            var queue = new Queue
            {
                QueueID = Guid.NewGuid(),
                ParentID = request.ParentID,
                QueueNumber = nextQueueNumber,
                QueueDate = today,
                Status = "Waiting",
                CreatedAt = DateTime.Now
            };

            foreach (var childId in request.ChildIDs.Distinct())
            {
                queue.QueueChildren.Add(new QueueChild
                {
                    QueueChildID = Guid.NewGuid(),
                    QueueID = queue.QueueID,
                    ChildID = childId,
                    CreatedAt = DateTime.Now
                });
            }

            var createdQueue =
                await _queueRepository.CreateAsync(queue);

            var result =
                await _queueRepository.GetByIdAsync(createdQueue.QueueID);

            return CreatedAtAction(
                nameof(GetById),
                new { id = createdQueue.QueueID },
                result
            );
        }

        // PUT: api/Queue/{id}/status
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(
            Guid id,
            [FromBody] UpdateQueueStatusRequest request)
        {
            var queue = await _queueRepository.GetByIdAsync(id);

            if (queue == null)
                return NotFound();

            queue.Status = request.Status;
            queue.UpdatedAt = DateTime.Now;

            await _queueRepository.UpdateAsync(queue);

            return Ok(queue);
        }

        // DELETE: api/Queue/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var queue = await _queueRepository.GetByIdAsync(id);

            if (queue == null)
                return NotFound();

            await _queueRepository.DeleteAsync(id);

            return NoContent();
        }
    }

    // ============================================
    // REQUEST DTOs
    // ============================================

    public class CreateQueueRequest
    {
        public Guid ParentID { get; set; }

        public List<Guid> ChildIDs { get; set; }
            = new List<Guid>();
    }

    public class UpdateQueueStatusRequest
    {
        public string Status { get; set; } = "Waiting";
    }
}