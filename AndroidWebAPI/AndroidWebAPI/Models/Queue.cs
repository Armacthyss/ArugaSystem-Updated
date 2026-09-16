using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AndroidWebAPI.Models
{
    [Table("Queues")]
    public class Queue
    {
        [Key]
        public Guid QueueID { get; set; }

        public Guid ParentID { get; set; }

        public int QueueNumber { get; set; }

        public DateTime QueueDate { get; set; }

        public string Status { get; set; } = "Waiting";

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }


        // ============================================
        // HEALTHCARE WORKER ASSIGNMENT
        // ============================================

        public Guid? AssignedWorkerID { get; set; }

        [ForeignKey(nameof(AssignedWorkerID))]
        public virtual User? AssignedWorker { get; set; }

        public string AssignmentStatus { get; set; } = "Unassigned";

        public DateTime? AssignedAt { get; set; }

        public DateTime? AssignmentRespondedAt { get; set; }


        // ============================================
        // NAVIGATION
        // ============================================

        [ForeignKey(nameof(ParentID))]
        public virtual Parent? Parent { get; set; }

        public virtual ICollection<QueueChild> QueueChildren { get; set; }
            = new List<QueueChild>();
    }
}