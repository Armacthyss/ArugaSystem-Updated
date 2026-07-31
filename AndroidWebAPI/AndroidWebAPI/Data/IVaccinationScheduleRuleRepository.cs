using AndroidWebAPI.Models;

namespace AndroidWebAPI.Data
{
    public interface IVaccinationScheduleRuleRepository
    {
        Task<IEnumerable<VaccinationScheduleRule>> GetAllAsync();
        Task<IEnumerable<VaccinationScheduleRule>> GetByVaccineAsync(int vaccineId);
        Task<VaccinationScheduleRule?> GetRuleAsync(int vaccineId, int doseNumber);

        Task AddAsync(VaccinationScheduleRule rule);
        Task UpdateAsync(VaccinationScheduleRule rule);
        Task DeleteAsync(int ruleId);
    }
}