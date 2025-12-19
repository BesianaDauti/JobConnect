using System;
using System.ComponentModel.DataAnnotations;

namespace JobConnect.Models
{
    public class JobApplication
    {
        [Key]
        public int ApplicationId { get; set; }

        public int JobId { get; set; }
        public Job Job { get; set; }

        public int JobSeekerId { get; set; }
        public JobSeekerProfile JobSeeker { get; set; }

        public DateTime AppliedAt { get; set; } = DateTime.UtcNow;

        public string Status { get; set; } = "Pending";
    }
}
