namespace JobConnect.Models
{
    public class Job
    {
        public int JobId { get; set; }
        public string Title { get; set; }
        public string CompanyName { get; set; }
        public string Location { get; set; }
        public string JobType { get; set; }   
        public DateTime PostedAt { get; set; } = DateTime.UtcNow;

        public string? Salary { get; set; }
        public string? WorkingHours { get; set; }
        public string? ExperienceLevel { get; set; }
        public string Description { get; set; }
        public string? Requirements { get; set; }
        public string? Responsibilities { get; set; }
        public int PositionsAvailable { get; set; }
        public int EmployerId { get; set; }
        public Employer Employer { get; set; }
        public DateTime Deadline { get; set; }
        public bool IsActive { get; set; } = true;

    }
}
