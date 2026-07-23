using System.ComponentModel.DataAnnotations;

namespace AndroidWebAPI.Models
{
    public class Account
    {
        [Key]
        public Guid AccountID { get; set; }

        public string Username { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string PasswordHash { get; set; } = string.Empty;

        /// <summary>
        /// Parent, Nurse, Doctor, Staff, SystemAdmin
        /// </summary>
        public string Role { get; set; } = string.Empty;

        /// <summary>
        /// Active, Inactive, Suspended
        /// </summary>
        public string Status { get; set; } = "Active";

        public DateTime? LastLogin { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }
    }
}