namespace JobConnect.Models
{
    public class JobSeekerProfile
    {
        public int JobSeekerProfileId { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string? LinkedIn { get; set; }
        public string? GitHub { get; set; }
        public string? Skills { get; set; }
        public int? ExperienceYears { get; set; }
        public string? ResumeFilePath { get; set; }
        public string? ProfessionalTitle { get; set; }   
        public string? Location { get; set; }            
        public string? AboutMe { get; set; }             
        public string? University { get; set; }
        public string? Degree { get; set; }
        public int? GraduationYear { get; set; }

    }
}
