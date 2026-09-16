using AndroidWebAPI.Data;
using AndroidWebAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace AndroidWebAPI.Repositories
{
    public class NotificationRuleRepository : INotificationRuleRepository
    {
        private readonly AppDbContext _context;

        public NotificationRuleRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<NotificationRule>> GetAllAsync()
        {
            return await _context.NotificationRules
                .OrderBy(r => r.NotificationType)
                .ThenBy(r => r.TriggerValue)
                .ToListAsync();
        }

        public async Task<NotificationRule?> GetByIdAsync(Guid ruleId)
        {
            return await _context.NotificationRules
                .FirstOrDefaultAsync(r => r.RuleID == ruleId);
        }

        public async Task<NotificationRule> CreateAsync(NotificationRule rule)
        {
            _context.NotificationRules.Add(rule);
            await _context.SaveChangesAsync();

            return rule;
        }

        public async Task<NotificationRule> UpdateAsync(NotificationRule rule)
        {
            _context.NotificationRules.Update(rule);
            await _context.SaveChangesAsync();

            return rule;
        }

        public async Task<bool> DeleteAsync(Guid ruleId)
        {
            var rule = await _context.NotificationRules
                .FirstOrDefaultAsync(r => r.RuleID == ruleId);

            if (rule == null)
                return false;

            _context.NotificationRules.Remove(rule);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}