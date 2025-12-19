namespace JobConnect.DTOs
{
    public class RegisterJobSeekerDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Password { get; set; }
        public string? LinkedIn { get; set; }
        public string? GitHub { get; set; }
        public string? Skills { get; set; }
        public int? ExperienceYears { get; set; }
        public IFormFile? ResumeFile { get; set; }
    }
}

