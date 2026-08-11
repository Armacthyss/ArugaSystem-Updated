using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AndroidWebAPI.Models
{
    [Table("VaccinationRecords")]
    public class VaccinationRecord
    {
        [Key]
        public Guid VaccinationRecordID { get; set; }

        public Guid ChildID { get; set; }

        public int VaccineID { get; set; }

        public int InventoryID { get; set; }

        public int DoseNumber { get; set; }

        public DateTime VaccinationDate { get; set; }

        public Guid AdministeredByUserID { get; set; }

        public string? NurseObservation { get; set; }

        public string? DoctorDiagnosis { get; set; }

        public DateTime? DoctorDiagnosedAt { get; set; }

        // Completed / Cancelled / Deferred
        public string Status { get; set; } = "Completed";

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        //------------------------------------
        // Navigation Properties
        //------------------------------------

        [ForeignKey(nameof(ChildID))]
        public virtual Child Child { get; set; }

        [ForeignKey(nameof(VaccineID))]
        public virtual Vaccine Vaccine { get; set; }

        [ForeignKey(nameof(InventoryID))]
        public virtual VaccineInventory Inventory { get; set; }

        [ForeignKey(nameof(AdministeredByUserID))]
        public virtual Personnel AdministeredBy { get; set; }
    }
}