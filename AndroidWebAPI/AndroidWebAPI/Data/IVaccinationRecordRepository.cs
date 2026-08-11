using AndroidWebAPI.Models;

namespace AndroidWebAPI.Data
{
    public interface IVaccinationRecordRepository
    {
        Task<IEnumerable<VaccinationRecord>> GetAllAsync();
        Task<VaccinationRecord?> GetByIdAsync(Guid vaccinationRecordId);
        Task<IEnumerable<VaccinationRecord>> GetByChildAsync(Guid childId);

        Task AddAsync(VaccinationRecord record);
        Task UpdateAsync(VaccinationRecord record);
        Task DeleteAsync(Guid vaccinationRecordId);


        Task<bool> AlreadyVaccinatedAsync(Guid childId, int vaccineId, int doseNumber);
   // Business Logic
Task RecordVaccinationAsync(VaccinationRecord record);
   
    }
}