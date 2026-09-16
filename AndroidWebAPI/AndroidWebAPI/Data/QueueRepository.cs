using AndroidWebAPI.Data;
using AndroidWebAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace AndroidWebAPI.Repositories
{
    public class QueueRepository : IQueueRepository
    {
        private readonly AppDbContext _context;

        public QueueRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Queue>> GetAllAsync()
        {
            return await _context.Queues
                .Include(q => q.Parent)
                .Include(q => q.AssignedWorker)
                .Include(q => q.QueueChildren)
                    .ThenInclude(qc => qc.Child)
                .OrderBy(q => q.QueueDate)
                .ThenBy(q => q.QueueNumber)
                .ToListAsync();
        }

        public async Task<Queue?> GetByIdAsync(Guid queueId)
        {
            return await _context.Queues
                .Include(q => q.Parent)
                .Include(q => q.AssignedWorker)
                .Include(q => q.QueueChildren)
                    .ThenInclude(qc => qc.Child)
                .FirstOrDefaultAsync(q => q.QueueID == queueId);
        }

public async Task<Queue?> GetParentQueueAsync(
    Guid parentId,
    DateTime queueDate)
{
    var date = queueDate.Date;

    return await _context.Queues
        .Include(q => q.Parent)
        .Include(q => q.AssignedWorker)
        .Include(q => q.QueueChildren)
            .ThenInclude(qc => qc.Child)
        .FirstOrDefaultAsync(q =>
            q.ParentID == parentId &&
            q.QueueDate == date);
}

public async Task<int> GetNextQueueNumberAsync(DateTime queueDate)
{
    var date = queueDate.Date;

    var lastQueueNumber = await _context.Queues
        .Where(q => q.QueueDate == date)
        .Select(q => (int?)q.QueueNumber)
        .MaxAsync();

    return (lastQueueNumber ?? 0) + 1;
}

public async Task<List<Queue>> GetTodayQueuesAsync()
{
    var today = DateTime.Today;

    return await _context.Queues
        .Include(q => q.Parent)
        .Include(q => q.AssignedWorker)
        .Include(q => q.QueueChildren)
            .ThenInclude(qc => qc.Child)
        .Where(q => q.QueueDate == today)
        .OrderBy(q => q.QueueNumber)
        .ToListAsync();
}

        public async Task<Queue> CreateAsync(Queue queue)
        {
            _context.Queues.Add(queue);
            await _context.SaveChangesAsync();

            return queue;
        }

        public async Task UpdateAsync(Queue queue)
        {
            _context.Queues.Update(queue);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid queueId)
        {
            var queue = await _context.Queues
                .FirstOrDefaultAsync(q => q.QueueID == queueId);

            if (queue == null)
                return;

            _context.Queues.Remove(queue);
            await _context.SaveChangesAsync();
        }

        public async Task<ClinicOperatingSchedule?> GetTodayOperatingScheduleAsync(
    DateTime date)
{
    int dayOfWeek = (int)date.DayOfWeek;

    return await _context.ClinicOperatingSchedules
        .FirstOrDefaultAsync(s =>
            s.DayOfWeek == dayOfWeek &&
            s.IsOpen &&
            s.IsActive);
}
    }
}