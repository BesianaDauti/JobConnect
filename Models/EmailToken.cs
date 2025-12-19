namespace JobConnect.Models
{
    public class EmailToken
    {
        public int EmailTokenId { get; set; }

        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public DateTime ExpireAt { get; set; }
        public bool IsUsed { get; set; } = false;
        public string Token { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;

        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? LinkedIn { get; set; }
        public string? GitHub { get; set; }
        public string? Skills { get; set; }
        public int? ExperienceYears { get; set; }
        public string? ResumeFilePath { get; set; }

        public string? CompanyName { get; set; }
        public string? Location { get; set; }
        public string? CompanySize { get; set; }

        public string? Website { get; set; }
        public string? Industry { get; set; }
        public string? Description { get; set; }
        public string? LinkedInCompany { get; set; }
        public string? LogoPath { get; set; }
    }


}
