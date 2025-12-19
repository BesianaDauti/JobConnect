namespace JobConnect.Models
{
    public class User
    {
        public int UserId { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string PasswordHash { get; set; }
        public string Role { get; set; } 
        public bool EmailVerified { get; set; } = false;
        public bool IsActive { get; set; } = false;
        public JobSeekerProfile JobSeekerProfile { get; set; }
        public Employer Employer { get; set; }
        public string? ResetToken { get; set; }
        public DateTime? ResetTokenExpires { get; set; }
    }
}
