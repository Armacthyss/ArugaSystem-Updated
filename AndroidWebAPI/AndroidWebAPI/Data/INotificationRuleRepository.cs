using AndroidWebAPI.Models;

namespace AndroidWebAPI.Repositories
{
    public interface INotificationRuleRepository
    {
        Task<List<NotificationRule>> GetAllAsync();
        Task<NotificationRule?> GetByIdAsync(Guid ruleId);
        Task<NotificationRule> CreateAsync(NotificationRule rule);
        Task<NotificationRule> UpdateAsync(NotificationRule rule);
        Task<bool> DeleteAsync(Guid ruleId);
    }
}