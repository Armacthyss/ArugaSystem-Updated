using AndroidWebAPI.Models;
using AndroidWebAPI.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace AndroidWebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationSettingsController : ControllerBase
    {
        private readonly INotificationSettingRepository _repository;

        public NotificationSettingsController(
            INotificationSettingRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var setting = await _repository.GetAsync();

            if (setting == null)
            {
                setting = new NotificationSetting();

                await _repository.CreateAsync(setting);
            }

            return Ok(setting);
        }

        [HttpPut]
        public async Task<IActionResult> Update(
            [FromBody] NotificationSetting setting)
        {
            var existing = await _repository.GetAsync();

            if (existing == null)
            {
                setting.SettingID = Guid.NewGuid();
                setting.CreatedAt = DateTime.Now;

                var created = await _repository.CreateAsync(setting);

                return Ok(created);
            }

            existing.AutomaticNotificationsEnabled =
                setting.AutomaticNotificationsEnabled;

            existing.DefaultSendingTime =
                setting.DefaultSendingTime;

            existing.InAppEnabled =
                setting.InAppEnabled;

            existing.SmsEnabled =
                setting.SmsEnabled;

            existing.EmailEnabled =
                setting.EmailEnabled;

            existing.UpdatedAt = DateTime.Now;

            var updated = await _repository.UpdateAsync(existing);

            return Ok(updated);
        }
    }
}