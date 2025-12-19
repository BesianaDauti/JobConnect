namespace JobConnect.DTOs
{
    public class JobCreateDto
    {
        public string Title { get; set; }
        public string Category { get; set; }
        public string JobType { get; set; }
        public string Location { get; set; }
        public DateTime Deadline { get; set; }
        public string? Salary { get; set; }
        public string? WorkingHours { get; set; }
        public string? ExperienceLevel { get; set; }
        public string Description { get; set; }
        public string? Requirements { get; set; }
        public string? Responsibilities { get; set; }
        public int PositionsAvailable { get; set; }
    }
}
