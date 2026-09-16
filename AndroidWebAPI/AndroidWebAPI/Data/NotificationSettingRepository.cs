using AndroidWebAPI.Data;
using AndroidWebAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace AndroidWebAPI.Repositories
{
    public class NotificationSettingRepository : INotificationSettingRepository
    {
        private readonly AppDbContext _context;

        public NotificationSettingRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<NotificationSetting?> GetAsync()
        {
            return await _context.NotificationSettings
                .FirstOrDefaultAsync();
        }

        public async Task<NotificationSetting> CreateAsync(NotificationSetting setting)
        {
            _context.NotificationSettings.Add(setting);
            await _context.SaveChangesAsync();

            return setting;
        }

        public async Task<NotificationSetting> UpdateAsync(NotificationSetting setting)
        {
            _context.NotificationSettings.Update(setting);
            await _context.SaveChangesAsync();

            return setting;
        }
    }
}
