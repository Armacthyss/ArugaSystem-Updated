using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AndroidWebAPI.Models
{
    [Table("NotificationRules")]
    public class NotificationRule
    {
        [Key]
        public Guid RuleID { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(150)]
        public string RuleName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string NotificationType { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string TriggerType { get; set; } = string.Empty;

        public int? TriggerValue { get; set; }

        public bool InAppEnabled { get; set; } = true;

        public bool SmsEnabled { get; set; } = false;

        public bool EmailEnabled { get; set; } = false;

        public bool IsEnabled { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }
    }
}