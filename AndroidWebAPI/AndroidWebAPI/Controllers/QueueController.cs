using AndroidWebAPI.Models;
using AndroidWebAPI.Repositories;
using AndroidWebAPI.Data;
using AndroidWebAPI.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AndroidWebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QueueController : ControllerBase
    {
        private readonly IQueueRepository _queueRepository;
        private readonly AppDbContext _context;

        public QueueController(
            IQueueRepository queueRepository,
            AppDbContext context)
        {
            _queueRepository = queueRepository;
            _context = context;
        }

        // GET: api/Queue
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var queues = await _queueRepository.GetAllAsync();

            var result = queues.Select(ToQueueResponse).ToList();

            return Ok(result);
        }

        // GET: api/Queue/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var queue = await _queueRepository.GetByIdAsync(id);

            if (queue == null)
                return NotFound();

            return Ok(ToQueueResponse(queue));
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

            return Ok(ToQueueResponse(queue));
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
            var now = DateTime.Now;
            var currentTime = now.TimeOfDay;

            var operatingSchedule =
                await _queueRepository.GetTodayOperatingScheduleAsync(today);

            if (operatingSchedule == null)
            {
                return BadRequest(new
                {
                    message = "The vaccination queue is closed today."
                });
            }

            if (currentTime < operatingSchedule.OpeningTime)
            {
                return BadRequest(new
                {
                    message = "The vaccination queue is not open yet.",
                    openingTime = operatingSchedule.OpeningTime
                });
            }

            if (currentTime > operatingSchedule.QueueCutoffTime)
            {
                return BadRequest(new
                {
                    message = "The vaccination queue is no longer accepting new patients today.",
                    queueCutoffTime = operatingSchedule.QueueCutoffTime
                });
            }

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

            if (nextQueueNumber > 999)
            {
                return BadRequest(new
                {
                    message = "Today's queue has reached the maximum queue number of Q-999."
                });
            }

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
                new { id = result!.QueueID },
                ToQueueResponse(result)
            );
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(
            Guid id,
            [FromBody] UpdateQueueStatusRequest request)
        {
            var queue = await _queueRepository.GetByIdAsync(id);

            if (queue == null)
                return NotFound();

            var allowedStatuses = new[]
            {
                "Waiting",
                "Called",
                "Cancelled",
                "NoShow"
            };

            if (!allowedStatuses.Contains(request.Status))
            {
                return BadRequest(new
                {
                    message = "Invalid queue status.",
                    allowedStatuses
                });
            }

            queue.Status = request.Status;

            if ((request.Status == "Cancelled" ||
                 request.Status == "NoShow") &&
                queue.AssignmentStatus == "Pending")
            {
                queue.AssignmentStatus = "Cancelled";
                queue.AssignmentRespondedAt = DateTime.Now;
            }

            queue.UpdatedAt = DateTime.Now;

            await _queueRepository.UpdateAsync(queue);

            return Ok(ToQueueResponse(queue));
        }

        [HttpPut("{id}/accept-assignment")]
        public async Task<IActionResult> AcceptAssignment(
            Guid id,
            [FromBody] WorkerAssignmentResponseRequest request)
        {
            var queue = await _queueRepository.GetByIdAsync(id);

            if (queue == null)
            {
                return NotFound(new
                {
                    message = "Queue entry not found."
                });
            }

            if (queue.AssignmentStatus != "Pending")
            {
                return BadRequest(new
                {
                    message = "This queue entry does not have a pending assignment."
                });
            }

            if (queue.AssignedWorkerID == null)
            {
                return BadRequest(new
                {
                    message = "No healthcare worker is assigned to this patient."
                });
            }

            if (queue.AssignedWorkerID != request.WorkerID)
            {
                return BadRequest(new
                {
                    message = "This assignment belongs to another healthcare worker."
                });
            }

            var worker = await _context.Users
                .FirstOrDefaultAsync(u =>
                    u.UserID == request.WorkerID);

            if (worker == null)
            {
                return NotFound(new
                {
                    message = "Healthcare worker not found."
                });
            }

            if (worker.AccountStatus != "Active")
            {
                return BadRequest(new
                {
                    message = "Healthcare worker account is not active."
                });
            }

            if (worker.AvailabilityStatus != "Online")
            {
                return BadRequest(new
                {
                    message = "Healthcare worker is not currently available."
                });
            }

            var now = DateTime.Now;

           queue.AssignmentStatus = "Accepted";
queue.AssignmentRespondedAt = now;
queue.UpdatedAt = now;

// Patient is now accepted by the HWR,
// but treatment has not started yet.
queue.Status = "Waiting";

// HWR is still available until they actually start handling the patient.
worker.AvailabilityStatus = "Online";

            await _queueRepository.UpdateAsync(queue);

            return Ok(new
            {
                message = "Assignment accepted successfully.",
                queueID = queue.QueueID,
                queueNumber = queue.QueueNumber,
                queueStatus = queue.Status,
                assignmentStatus = queue.AssignmentStatus,
                workerID = worker.UserID,
                workerName = $"{worker.FirstName} {worker.LastName}".Trim(),
                availabilityStatus = worker.AvailabilityStatus,
                respondedAt = queue.AssignmentRespondedAt
            });
        }

        [HttpGet("worker/{workerId}/pending")]
        public async Task<IActionResult> GetPendingAssignments(Guid workerId)
        {
            var worker = await _context.Users
                .FirstOrDefaultAsync(u =>
                    u.UserID == workerId);

            if (worker == null)
            {
                return NotFound(new
                {
                    message = "Healthcare worker not found."
                });
            }

            var today = DateTime.Today;

            var queues = await _context.Queues
                .Include(q => q.Parent)
                .Include(q => q.AssignedWorker)
                .Include(q => q.QueueChildren)
                    .ThenInclude(qc => qc.Child)
                .Where(q =>
                    q.QueueDate == today &&
                    q.AssignedWorkerID == workerId &&
                    q.AssignmentStatus == "Pending" &&
                    (q.Status == "Waiting" ||
                     q.Status == "Called"))
                .OrderBy(q => q.QueueNumber)
                .ToListAsync();

            var result = queues
                .Select(ToQueueResponse)
                .ToList();

            return Ok(result);
        }

        [HttpPut("{id}/assign")]
        public async Task<IActionResult> AssignWorker(
            Guid id,
            [FromBody] AssignQueueRequest request)
        {
            var queue = await _queueRepository.GetByIdAsync(id);

            if (queue == null)
            {
                return NotFound(new
                {
                    message = "Queue entry not found."
                });
            }

            // Only active queue patients may be assigned
          if (queue.Status != "Waiting")
{
    return BadRequest(new
    {
        message = "Only waiting queue entries can be assigned."
    });
}

            // Prevent duplicate pending assignment
            if (queue.AssignmentStatus == "Pending")
            {
                return Conflict(new
                {
                    message = "This patient is already waiting for healthcare worker confirmation."
                });
            }

            var worker = await _context.Users
                .FirstOrDefaultAsync(u =>
                    u.UserID == request.WorkerID);

            if (worker == null)
            {
                return NotFound(new
                {
                    message = "Healthcare worker not found."
                });
            }

            if (worker.Position != "Doctor" &&
                worker.Position != "Nurse" &&
                worker.Position != "Nurse/Midwife")
            {
                return BadRequest(new
                {
                    message = "Selected user is not a healthcare worker."
                });
            }

            if (worker.AccountStatus != "Active")
            {
                return BadRequest(new
                {
                    message = "Healthcare worker account is not active."
                });
            }

            if (worker.AvailabilityStatus != "Online")
            {
                return BadRequest(new
                {
                    message = "Healthcare worker is not currently available."
                });
            }

            // Assignment is only a request.
            // Worker is NOT occupied yet.
            queue.AssignedWorkerID = worker.UserID;
            queue.AssignmentStatus = "Pending";
            queue.AssignedAt = DateTime.Now;
            queue.AssignmentRespondedAt = null;
            queue.UpdatedAt = DateTime.Now;

            await _queueRepository.UpdateAsync(queue);

            return Ok(new
            {
                message = "Healthcare worker assignment sent for confirmation.",
                queueID = queue.QueueID,
                queueNumber = queue.QueueNumber,
                workerID = worker.UserID,
                workerName = $"{worker.FirstName} {worker.LastName}".Trim(),
                queueStatus = queue.Status,
                assignmentStatus = queue.AssignmentStatus,
                availabilityStatus = worker.AvailabilityStatus,
                assignedAt = queue.AssignedAt
            });
        }

        [HttpPut("{id}/decline-assignment")]
        public async Task<IActionResult> DeclineAssignment(
            Guid id,
            [FromBody] WorkerAssignmentResponseRequest request)
        {
            var queue = await _queueRepository.GetByIdAsync(id);

            if (queue == null)
            {
                return NotFound(new
                {
                    message = "Queue entry not found."
                });
            }

            if (queue.AssignmentStatus != "Pending")
            {
                return BadRequest(new
                {
                    message = "This queue entry does not have a pending assignment."
                });
            }

            if (queue.AssignedWorkerID != request.WorkerID)
            {
                return BadRequest(new
                {
                    message = "This assignment belongs to another healthcare worker."
                });
            }

            var worker = await _context.Users
                .FirstOrDefaultAsync(u =>
                    u.UserID == request.WorkerID);

            if (worker == null)
            {
                return NotFound(new
                {
                    message = "Healthcare worker not found."
                });
            }

          queue.AssignmentStatus = "Declined";
queue.AssignmentRespondedAt = DateTime.Now;

queue.AssignedWorkerID = null;
queue.AssignedAt = null;

queue.UpdatedAt = DateTime.Now;

            // Queue stays Waiting/Called.
            // Worker also stays Online.
            await _queueRepository.UpdateAsync(queue);

            return Ok(new
            {
                message = "Assignment declined.",
                queueID = queue.QueueID,
                queueNumber = queue.QueueNumber,
                queueStatus = queue.Status,
                assignmentStatus = queue.AssignmentStatus,
                workerID = worker.UserID,
                availabilityStatus = worker.AvailabilityStatus
            });
        }

        // ============================================
        // HELPERS
        // ============================================

        private static QueueResponseDto ToQueueResponse(Queue q)
        {
            return new QueueResponseDto
            {
                QueueID = q.QueueID,
                QueueNumber = q.QueueNumber,
                BarangayNo = q.Parent?.BarangayNo,
                RequestBy = $"{q.Parent?.FirstName} {q.Parent?.LastName}".Trim(),
               Status = q.Status,
QueueDate = q.QueueDate,

AssignedWorkerID = q.AssignedWorkerID,

AssignedWorkerName = q.AssignedWorker == null
    ? null
    : $"{q.AssignedWorker.FirstName} {q.AssignedWorker.LastName}".Trim(),

AssignmentStatus = q.AssignmentStatus,

AssignedAt = q.AssignedAt,

AssignmentRespondedAt = q.AssignmentRespondedAt,

Children = q.QueueChildren
    .Where(qc => qc.Child != null)
    .Select(qc => new QueueChildResponseDto
    {
        ChildID = qc.ChildID,
        Name = $"{qc.Child!.FirstName} {qc.Child.LastName}".Trim()
    })
    .ToList()
            };
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

    public class WorkerAssignmentResponseRequest
    {
        public Guid WorkerID { get; set; }
    }

    public class AssignQueueRequest
    {
        public Guid WorkerID { get; set; }
    }
}