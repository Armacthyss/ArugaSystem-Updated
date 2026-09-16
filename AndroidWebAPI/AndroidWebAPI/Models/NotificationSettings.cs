using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AndroidWebAPI.Models
{
    [Table("NotificationSettings")]
    public class NotificationSetting
    {
        [Key]
        public Guid SettingID { get; set; } = Guid.NewGuid();

        public bool AutomaticNotificationsEnabled { get; set; } = true;

        public TimeSpan DefaultSendingTime { get; set; }
            = new TimeSpan(8, 0, 0);

        public bool InAppEnabled { get; set; } = true;

        public bool SmsEnabled { get; set; } = false;

        public bool EmailEnabled { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }
    }
}