using AndroidWebAPI.Models;

namespace AndroidWebAPI.Repositories
{
    public interface INotificationSettingRepository
    {
        Task<NotificationSetting?> GetAsync();
        Task<NotificationSetting> CreateAsync(NotificationSetting setting);
        Task<NotificationSetting> UpdateAsync(NotificationSetting setting);
    }
}