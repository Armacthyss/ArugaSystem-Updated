using AndroidWebAPI.Models;

namespace AndroidWebAPI.Data
{
    public interface IVaccineTimelineRepository
    {
        Task<IEnumerable<VaccineTimeline>> GetAllAsync();
        Task<VaccineTimeline?> GetByIdAsync(Guid timelineId);
        Task<IEnumerable<VaccineTimeline>> GetByChildAsync(Guid childId);

        Task AddAsync(VaccineTimeline timeline);
        Task UpdateAsync(VaccineTimeline timeline);
        Task DeleteAsync(Guid timelineId);

        Task<bool> ExistsAsync(Guid childId, int vaccineId, int doseNumber);
    }
}