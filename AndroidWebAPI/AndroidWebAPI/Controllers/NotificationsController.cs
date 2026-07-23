using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AndroidWebAPI.Data;
using AndroidWebAPI.Models;

[ApiController]
[Route("api/[controller]")]
public class NotificationsController : ControllerBase
{
    private readonly AppDbContext _context;
    public NotificationsController(AppDbContext context) => _context = context;

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
        var notification = await _context.Notifications.FindAsync(notificationId);
        if (notification == null) return NotFound();
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
}


// Models/Notification.cs
// Add this to your Models folder
public class Notification
{
    public Guid   NotificationID { get; set; } = Guid.NewGuid();
    public Guid   ParentID       { get; set; }
    public Guid   ChildID        { get; set; }
    public int?   VaccineID      { get; set; }
    public int?   DoseNumber     { get; set; }

    // Type values: "ReminderMonth" | "ReminderWeek" | "Reminder5Day" | "ReminderDay" | "StockAlert" | "StockResolved"
    public string Type           { get; set; } = string.Empty;
    public string Title          { get; set; } = string.Empty;
    public string Message        { get; set; } = string.Empty;
    public DateTime? ScheduledDate { get; set; }
    public bool   IsRead         { get; set; } = false;
    public DateTime CreatedAt    { get; set; } = DateTime.Now;
}

