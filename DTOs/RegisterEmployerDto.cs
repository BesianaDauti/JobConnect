namespace JobConnect.DTOs
{
    public class RegisterEmployerDto
    {
        public string CompanyName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Password { get; set; }
        public string Location { get; set; }
        public string CompanySize { get; set; }

        public string? Website { get; set; }
        public string? Industry { get; set; }
        public string? Description { get; set; }
        public string? LinkedInCompany { get; set; }
        public IFormFile? LogoFile { get; set; }
    }
}
