using System.ComponentModel.DataAnnotations;

namespace AndroidWebAPI.Models
{
    public class Child
    {
        [Key]
        public Guid ChildID { get; set; }

        public string FirstName { get; set; } = string.Empty;

        public string? MiddleName { get; set; }

        public string LastName { get; set; } = string.Empty;

        public DateTime BirthDate { get; set; }

        public string? PlaceOfBirth { get; set; }

        public string? Address { get; set; }

        public string? HealthCenter { get; set; }
        public int? Barangay { get; set; }

        public string? Sex { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation Property
        public virtual ICollection<ChildParentRelationship> ParentRelationships { get; set; }
            = new List<ChildParentRelationship>();
    }
}