namespace AndroidWebAPI.Models
{
    public class VaccineDose
    {
        public int DoseID { get; set; }
        public int VaccineID { get; set; }
        public int DoseNumber { get; set; }
        public int MinIntervalDays { get; set; }

        // Navigation property
        public Vaccine? Vaccine { get; set; }
    }
}