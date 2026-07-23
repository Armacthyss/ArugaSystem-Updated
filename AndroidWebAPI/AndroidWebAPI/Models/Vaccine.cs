// Models/Vaccine.cs
// Maps to dbo.Vaccines — columns: VaccineID, VaccineName, Description
// NOTE: MinIntervalDays is in dbo.VaccineDoses, NOT here.
using System.ComponentModel.DataAnnotations;

namespace AndroidWebAPI.Models
{
    public class Vaccine
    {
        [Key]
        public int VaccineID { get; set; }

        public string VaccineName { get; set; } = string.Empty;

        public string? Description { get; set; }
    }
}