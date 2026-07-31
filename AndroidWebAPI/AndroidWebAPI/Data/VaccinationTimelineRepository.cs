using AndroidWebAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace AndroidWebAPI.Data.Repositories
{
    public class VaccineTimelineRepository : IVaccineTimelineRepository
    {
        private readonly AppDbContext _context;

        public VaccineTimelineRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<VaccineTimeline>> GetAllAsync()
        {
            return await _context.VaccineTimelines
                .Include(t => t.Child)
                .Include(t => t.Vaccine)
                .ToListAsync();
        }

        public async Task<VaccineTimeline?> GetByIdAsync(Guid timelineId)
        {
            return await _context.VaccineTimelines
                .Include(t => t.Child)
                .Include(t => t.Vaccine)
                .FirstOrDefaultAsync(t => t.TimelineID == timelineId);
        }

        public async Task<IEnumerable<VaccineTimeline>> GetByChildAsync(Guid childId)
        {
            return await _context.VaccineTimelines
                .Where(t => t.ChildID == childId)
                .OrderBy(t => t.ExpectedDate)
                .ToListAsync();
        }

        public async Task AddAsync(VaccineTimeline timeline)
        {
            await _context.VaccineTimelines.AddAsync(timeline);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(VaccineTimeline timeline)
        {
            _context.VaccineTimelines.Update(timeline);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid timelineId)
        {
            var timeline = await _context.VaccineTimelines.FindAsync(timelineId);

            if (timeline != null)
            {
                _context.VaccineTimelines.Remove(timeline);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(Guid childId, int vaccineId, int doseNumber)
        {
            return await _context.VaccineTimelines.AnyAsync(t =>
                t.ChildID == childId &&
                t.VaccineID == vaccineId &&
                t.DoseNumber == doseNumber);
        }
    }
}