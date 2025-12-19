using Microsoft.AspNetCore.Http;

namespace JobConnect.DTOs
{
    public class UpdateProfileDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Phone { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? LinkedIn { get; set; }
        public string? GitHub { get; set; }
        public string? Skills { get; set; }
        public int? ExperienceYears { get; set; }
        public string? ProfessionalTitle { get; set; }
        public string? Location { get; set; }
        public string? AboutMe { get; set; }
        public string? University { get; set; }
        public string? Degree { get; set; }
        public int? GraduationYear { get; set; }
        public IFormFile? ResumeFile { get; set; }
    }
}
