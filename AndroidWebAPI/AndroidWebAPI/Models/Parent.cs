using System.ComponentModel.DataAnnotations;

namespace AndroidWebAPI.Models
{
    public class Parent
    {
        [Key]
        public Guid ParentID { get; set; }

        public string FirstName { get; set; } = string.Empty;

        public string? MiddleName { get; set; }

        public string LastName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string ContactNo { get; set; } = string.Empty;

        public string? BarangayNo { get; set; }

        public string? Address { get; set; }

        public string Password { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }

        // Navigation Property
        public virtual ICollection<ChildParentRelationship> ChildRelationships { get; set; }
            = new List<ChildParentRelationship>();
    }
}