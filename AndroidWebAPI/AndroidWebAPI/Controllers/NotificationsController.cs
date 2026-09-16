using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AndroidWebAPI.Data;
using AndroidWebAPI.Models;

[ApiController]
[Route("api/[controller]")]
public class NotificationsController : ControllerBase
{
    private readonly AppDbContext _context;

    public NotificationsController(AppDbContext context)
    {
        _context = context;
    }

    // GET api/Notifications/parent/{parentId}
    // All notifications for a parent, newest first
    [HttpGet("parent/{parentId}")]
    public async Task<IActionResult> GetByParent(Guid parentId)
    {
        var notifications = await _context.Notifications
            .Where(n => n.ParentID == parentId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();

        return Ok(notifications);
    }

    // GET api/Notifications/unread-count/{parentId}
    [HttpGet("unread-count/{parentId}")]
    public async Task<IActionResult> GetUnreadCount(Guid parentId)
    {
        int count = await _context.Notifications
            .CountAsync(n => n.ParentID == parentId && !n.IsRead);

        return Ok(new { count });
    }

    // PATCH api/Notifications/mark-read/{notificationId}
    [HttpPatch("mark-read/{notificationId}")]
    public async Task<IActionResult> MarkRead(Guid notificationId)
    {
        var notification = await _context.Notifications
            .FindAsync(notificationId);

        if (notification == null)
            return NotFound();

        notification.IsRead = true;

        await _context.SaveChangesAsync();

        return Ok();
    }

    // PATCH api/Notifications/mark-all-read/{parentId}
    [HttpPatch("mark-all-read/{parentId}")]
    public async Task<IActionResult> MarkAllRead(Guid parentId)
    {
        var unread = await _context.Notifications
            .Where(n => n.ParentID == parentId && !n.IsRead)
            .ToListAsync();

        unread.ForEach(n => n.IsRead = true);

        await _context.SaveChangesAsync();

        return Ok(new { marked = unread.Count });
    }

    [HttpGet("admin")]
public async Task<IActionResult> GetAdminNotifications()
{
    var notifications = await (
        from n in _context.Notifications
        join p in _context.Parents
            on n.ParentID equals p.ParentID
        join c in _context.Children
            on n.ChildID equals c.ChildID into childGroup
        from c in childGroup.DefaultIfEmpty()
        orderby n.CreatedAt descending
        select new
        {
            id = n.NotificationID,
            title = n.Title,
            recipient = p.FirstName + " " + p.LastName,
            child = c == null
                ? "—"
                : c.FirstName + " " + c.LastName,
            type = n.Type,
            scheduledDate = n.ScheduledDate,
            sentDate = n.SentAt,
            status = n.Status,
            message = n.Message,
            deliveryMethod = n.DeliveryMethod,
            opened = n.IsRead,
            createdAt = n.CreatedAt
        }
    ).ToListAsync();

    return Ok(notifications);
}

    // POST api/Notifications/announcement
    // Creates an announcement notification for all parents.
    [HttpPost("announcement")]
    public async Task<IActionResult> CreateAnnouncement(
        [FromBody] CreateAnnouncementRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            return BadRequest(new { message = "Title is required." });

        if (string.IsNullOrWhiteSpace(request.Message))
            return BadRequest(new { message = "Message is required." });

        if (request.DeliveryMethod != "In-App")
            return BadRequest(new { message = "Only In-App notifications are currently supported." });

        var parents = await _context.Parents
            .ToListAsync();

        if (parents.Count == 0)
            return BadRequest(new { message = "No parents are available to receive the announcement." });

        bool isScheduled = request.ScheduledDate.HasValue &&
                           request.ScheduledDate.Value > DateTime.Now;

        var notifications = parents.Select(parent => new Notification
        {
            NotificationID = Guid.NewGuid(),

            ParentID = parent.ParentID,
            ChildID = null,

            VaccineID = null,
            DoseNumber = null,

            Type = "Announcement",
            Title = request.Title.Trim(),
            Message = request.Message.Trim(),

            ScheduledDate = isScheduled
                ? request.ScheduledDate
                : null,

            SentAt = isScheduled
                ? null
                : DateTime.Now,

            Status = isScheduled
                ? "Scheduled"
                : "Sent",

            DeliveryMethod = "In-App",

            IsRead = false,
            CreatedAt = DateTime.Now
        }).ToList();

        _context.Notifications.AddRange(notifications);

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = isScheduled
                ? "Announcement scheduled successfully."
                : "Announcement sent successfully.",
            created = notifications.Count,
            status = isScheduled ? "Scheduled" : "Sent"
        });
    }
}

public class CreateAnnouncementRequest
{
    public string Title { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public string Recipients { get; set; } = "All Parents";

    public string DeliveryMethod { get; set; } = "In-App";

    public DateTime? ScheduledDate { get; set; }
}