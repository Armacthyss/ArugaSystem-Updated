using AndroidWebAPI.Models;
using Microsoft.EntityFrameworkCore;
using AndroidWebAPI.DTOs;

namespace AndroidWebAPI.Data
{
    public class VaccinationTimelineRepository : IVaccinationTimelineRepository
    {
        private readonly AppDbContext _context;

        public VaccinationTimelineRepository(AppDbContext context)
        {
            _context = context;
        }

        // ============================
        // CRUD
        // ============================

        public async Task<IEnumerable<VaccinationTimeline>> GetAllAsync()
        {
            return await _context.VaccinationTimelines
                .Include(t => t.Child)
                .Include(t => t.Vaccine)
                .ToListAsync();
        }

        public async Task<VaccinationTimeline?> GetByIdAsync(Guid timelineId)
        {
            return await _context.VaccinationTimelines
                .Include(t => t.Child)
                .Include(t => t.Vaccine)
                .FirstOrDefaultAsync(t => t.TimelineID == timelineId);
        }

        public async Task<IEnumerable<VaccinationTimeline>> GetByChildAsync(Guid childId)
        {
            return await _context.VaccinationTimelines
                .Where(t => t.ChildID == childId)
                .OrderBy(t => t.ExpectedDate)
                .ToListAsync();
        }

        public async Task AddAsync(VaccinationTimeline timeline)
        {
            await _context.VaccinationTimelines.AddAsync(timeline);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(VaccinationTimeline timeline)
        {
            _context.VaccinationTimelines.Update(timeline);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid timelineId)
        {
            var timeline = await _context.VaccinationTimelines.FindAsync(timelineId);

            if (timeline != null)
            {
                _context.VaccinationTimelines.Remove(timeline);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(Guid childId, int vaccineId, int doseNumber)
        {
            return await _context.VaccinationTimelines.AnyAsync(t =>
                t.ChildID == childId &&
                t.VaccineID == vaccineId &&
                t.DoseNumber == doseNumber);
        }

        // ============================
        // BUSINESS LOGIC
        // ============================

        public async Task GenerateTimelineAsync(Guid childId)
        {
            // 1. Get child
            var child = await _context.Children
                .FirstOrDefaultAsync(c => c.ChildID == childId);

            if (child == null)
                throw new Exception("Child not found.");

            // 2. Prevent duplicate timeline generation
            bool timelineExists = await _context.VaccinationTimelines
    .AnyAsync(t =>
        t.ChildID == childId &&
        t.Status == "Pending");

            if (timelineExists)
                throw new Exception("Vaccination timeline already exists for this child.");

            // 3. Load vaccination schedule rules
            var rules = await _context.VaccinationScheduleRules
                .OrderBy(r => r.SequenceOrder)
                .ToListAsync();

            if (!rules.Any())
                throw new Exception("No vaccination schedule rules found.");

            // 4. Generate timeline
            var timelines = new List<VaccinationTimeline>();

            foreach (var rule in rules)
            {
                var expectedDate = child.BirthDate.AddDays(rule.RecommendedAgeDays);

                timelines.Add(new VaccinationTimeline
                {
                    TimelineID = Guid.NewGuid(),
                    ChildID = child.ChildID,
                    VaccineID = rule.VaccineID,
                    DoseNumber = rule.DoseNumber,

                    // Expected schedule based on DOH recommendation
                    ExpectedDate = expectedDate,

                    // Temporary:
                    // Later this will adjust to clinic schedule
                    ScheduledDate = expectedDate,

                    Status = "Pending",

                    VaccinationRecordID = null,

                    CreatedAt = DateTime.UtcNow
                });
            }

            // 5. Save all timelines
            await _context.VaccinationTimelines.AddRangeAsync(timelines);
            await _context.SaveChangesAsync();
        }
        public async Task MarkCompletedAsync(Guid timelineId)
{
    var timeline = await _context.VaccinationTimelines
        .FirstOrDefaultAsync(t => t.TimelineID == timelineId);

    if (timeline == null)
        throw new Exception("Vaccination timeline not found.");

    timeline.Status = "Completed";
    timeline.UpdatedAt = DateTime.Now;

    _context.VaccinationTimelines.Update(timeline);
    await _context.SaveChangesAsync();
}
public async Task<IEnumerable<VaccinationTimeline>> GetDueTodayAsync()
{
    var today = DateTime.Today;

    return await _context.VaccinationTimelines
        .Include(t => t.Child)
        .Include(t => t.Vaccine)
        .Where(t =>
            t.Status == "Pending" &&
            t.ScheduledDate.Date == today)
        .OrderBy(t => t.ScheduledDate)
        .ToListAsync();
}

public async Task<IEnumerable<VaccinationTimeline>> GetUpcomingAsync(int days)
{
    var today = DateTime.Today;
    var endDate = today.AddDays(days);

    return await _context.VaccinationTimelines
        .Include(t => t.Child)
        .Include(t => t.Vaccine)
        .Where(t =>
            t.Status == "Pending" &&
            t.ScheduledDate.Date >= today &&
            t.ScheduledDate.Date <= endDate)
        .OrderBy(t => t.ScheduledDate)
        .ToListAsync();
}

public async Task RegenerateTimelineAsync(Guid childId)
{
    // Get the child
    var child = await _context.Children
        .FirstOrDefaultAsync(c => c.ChildID == childId);

    if (child == null)
        throw new Exception("Child not found.");

    // Delete only pending timeline records
    var pendingTimelines = await _context.VaccinationTimelines
        .Where(t => t.ChildID == childId &&
                    t.Status == "Pending")
        .ToListAsync();

    _context.VaccinationTimelines.RemoveRange(pendingTimelines);
    await _context.SaveChangesAsync();

    // Generate a new timeline
    await GenerateTimelineAsync(childId);
}
public async Task<TimelineSummaryDto> GetTimelineSummaryAsync(Guid childId)
{
    var timelines = await _context.VaccinationTimelines
        .Where(t => t.ChildID == childId)
        .ToListAsync();

    return new TimelineSummaryDto
    {
        ChildID = childId,
        HasTimeline = timelines.Any(),
        Pending = timelines.Count(t => t.Status == "Pending"),
        Completed = timelines.Count(t => t.Status == "Completed"),
        Missed = timelines.Count(t => t.Status == "Missed")
    };
}
    }
}