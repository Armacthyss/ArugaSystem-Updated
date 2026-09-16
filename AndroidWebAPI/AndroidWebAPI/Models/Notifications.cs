namespace AndroidWebAPI.Models
{
    public class Notification
    {
        public Guid NotificationID { get; set; } = Guid.NewGuid();

        public Guid ParentID { get; set; }

        public Guid? ChildID { get; set; }

        public int? VaccineID { get; set; }

        public int? DoseNumber { get; set; }

        public string Type { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        public DateTime? ScheduledDate { get; set; }

        public DateTime? SentAt { get; set; }

        public string Status { get; set; } = "Pending";

        public string DeliveryMethod { get; set; } = "In-App";

        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}