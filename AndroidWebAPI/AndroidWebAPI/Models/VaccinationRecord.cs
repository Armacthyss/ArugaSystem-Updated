// Models/VaccinationRecord.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AndroidWebAPI.Models
{
    public class VaccinationRecord
    {
        [Key]
        public Guid RecordID { get; set; }

        [Required]
        public Guid ChildID { get; set; }

        [Required]
        public int VaccineID { get; set; }

        [Required]
        public int DoseNumber { get; set; }

        // Nullable — DB allows NULL for unscheduled/pending records
        public DateTime? DateAdministered { get; set; }

        // All string columns must be nullable (?) because DB has NULL rows
        public Guid? AdministeredBy { get; set; }
        public string? AdministeredByName { get; set; }
        public string? LotNumber { get; set; }
        public string? Status { get; set; }
        public string? Remarks { get; set; }

        // These columns exist in your DB — add them so the model matches exactly
        public DateTime? ScheduledDate { get; set; }
    }
}