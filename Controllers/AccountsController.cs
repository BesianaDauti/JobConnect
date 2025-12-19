using BCrypt.Net;
using JobConnect.Data;
using JobConnect.DTOs;
using JobConnect.Interfaces;
using JobConnect.Models;
using JobConnect.Repositories;
using JobConnect.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace JobConnect.Controllers
{
    [NoCache]
    public class AccountsController : Controller
    {
        private readonly IUserRepository _userRepo;
        private readonly IJobSeekerRepository _jobSeekerRepo;
        private readonly IEmployerRepository _employerRepo;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _config;
        private readonly AppDbContext _db; 

        public AccountsController(
            AppDbContext db,
            IUserRepository userRepo,
            IJobSeekerRepository jobSeekerRepo,
            IEmployerRepository employerRepo,
            IEmailService emailService,
            IConfiguration config)
        {
            _db = db;
            _userRepo = userRepo;
            _jobSeekerRepo = jobSeekerRepo;
            _employerRepo = employerRepo;
            _emailService = emailService;
            _config = config;
        }

        public IActionResult Login() => View();

        [HttpPost]
        [EnableRateLimiting("login")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login([FromForm] LoginDto dto)
        {
            var user = await _userRepo.GetByEmailAsync(dto.Email);

            if (user == null)
                return BadRequest(new { message = "Invalid email or password." });

            if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return BadRequest(new { message = "Invalid email or password." });

            if (!user.EmailVerified)
                return BadRequest(new { message = "Please verify your email before logging in." });

            HttpContext.Session.SetInt32("UserId", user.UserId);
            HttpContext.Session.SetString("Role", user.Role);

            string redirect =
                user.Role == "Admin" ? Url.Action("Dashboard", "Admin") :
                user.Role == "Employer" ? Url.Action("Index", "Home") :
                Url.Action("Index", "Home");

            return Ok(new { redirect });
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
        public IActionResult Register()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> RegisterJobSeeker([FromForm] RegisterJobSeekerDto dto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto.Email) ||
                    string.IsNullOrWhiteSpace(dto.Password) ||
                    string.IsNullOrWhiteSpace(dto.FirstName) ||
                    string.IsNullOrWhiteSpace(dto.LastName) ||
                    string.IsNullOrWhiteSpace(dto.Phone))
                {
                    return BadRequest(new { message = "Please fill in all required fields." });
                }

                if (await _userRepo.GetByEmailAsync(dto.Email) != null)
                    return BadRequest(new { message = "This email is already in use." });

                if (await _userRepo.GetByPhoneAsync(dto.Phone) != null)
                    return BadRequest(new { message = "This phone number is already registered." });

                if (dto.Password.Length < 8)
                    return BadRequest(new { message = "Password must be at least 8 characters." });

                var old = await _db.EmailTokens.FirstOrDefaultAsync(x => x.Email == dto.Email);
                if (old != null) _db.EmailTokens.Remove(old);

                string? resumePath = null;

                if (dto.ResumeFile != null)
                {
                    var folder = Path.Combine("wwwroot", "temp_resumes");
                    if (!Directory.Exists(folder))
                        Directory.CreateDirectory(folder);

                    string fileName = $"{Guid.NewGuid()}_{dto.ResumeFile.FileName.Replace(" ", "_")}";
                    string path = Path.Combine(folder, fileName);

                    using var stream = new FileStream(path, FileMode.Create);
                    await dto.ResumeFile.CopyToAsync(stream);

                    resumePath = "/temp_resumes/" + fileName;
                }

                string token = Guid.NewGuid().ToString("N");

                var temp = new EmailToken
                {
                    Email = dto.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                    Role = "JobSeeker",

                    FirstName = dto.FirstName,
                    LastName = dto.LastName,
                    Phone = dto.Phone,

                    LinkedIn = dto.LinkedIn,
                    GitHub = dto.GitHub,
                    Skills = dto.Skills,
                    ExperienceYears = dto.ExperienceYears,
                    ResumeFilePath = resumePath,

                    Token = token,
                    ExpireAt = DateTime.UtcNow.AddMinutes(5),
                    IsUsed = false
                };

                await _db.EmailTokens.AddAsync(temp);
                await _db.SaveChangesAsync();

                await _emailService.SendVerificationEmail(dto.Email, token);

                return Ok(new
                {
                    message = "Please check your email to verify your account.",
                    redirect = Url.Action("Login", "Accounts")
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }


        [HttpPost]
        public async Task<IActionResult> RegisterEmployer([FromForm] RegisterEmployerDto dto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto.Email) ||
                    string.IsNullOrWhiteSpace(dto.Password) ||
                    string.IsNullOrWhiteSpace(dto.CompanyName) ||
                    string.IsNullOrWhiteSpace(dto.Phone) ||
                    string.IsNullOrWhiteSpace(dto.Location) ||
                    string.IsNullOrWhiteSpace(dto.CompanySize))
                {
                    return BadRequest(new { message = "Please fill in all required fields." });
                }

                if (await _userRepo.GetByEmailAsync(dto.Email) != null)
                    return BadRequest(new { message = "Email already in use." });

                if (await _userRepo.GetByPhoneAsync(dto.Phone) != null)
                    return BadRequest(new { message = "Phone number already in use." });

                bool companyExists = await _employerRepo.CompanyNameExistsAsync(dto.CompanyName);
                if (companyExists)
                    return BadRequest(new { message = "Company name already exists." });

                if (dto.Password.Length < 8)
                    return BadRequest(new { message = "Password must be at least 8 characters." });

                var old = await _db.EmailTokens.FirstOrDefaultAsync(x => x.Email == dto.Email);
                if (old != null) _db.EmailTokens.Remove(old);

                string? logoPath = null;

                if (dto.LogoFile != null)
                {
                    var folder = Path.Combine("wwwroot", "temp_logos");
                    if (!Directory.Exists(folder))
                        Directory.CreateDirectory(folder);

                    string fileName = $"{Guid.NewGuid()}_{dto.LogoFile.FileName}";
                    string path = Path.Combine(folder, fileName);

                    using var stream = new FileStream(path, FileMode.Create);
                    await dto.LogoFile.CopyToAsync(stream);

                    logoPath = "/temp_logos/" + fileName;
                }

                string token = Guid.NewGuid().ToString("N");

                var temp = new EmailToken
                {
                    Email = dto.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                    Phone = dto.Phone,
                    Role = "Employer",

                    CompanyName = dto.CompanyName,
                    Location = dto.Location,
                    CompanySize = dto.CompanySize,
                    Website = dto.Website,
                    Industry = dto.Industry,
                    Description = dto.Description,
                    LinkedInCompany = dto.LinkedInCompany,
                    LogoPath = logoPath,

                    Token = token,
                    ExpireAt = DateTime.UtcNow.AddMinutes(5)
                };

                await _db.EmailTokens.AddAsync(temp);
                await _db.SaveChangesAsync();

                await _emailService.SendVerificationEmail(dto.Email, token);

                return Ok(new
                {
                    message = "Please check your email to verify your account.",
                    redirect = Url.Action("Login", "Accounts")
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        public async Task<IActionResult> VerifyEmail(string token)
        {
            var pending = await _db.EmailTokens.FirstOrDefaultAsync(t => t.Token == token);

            if (pending == null || pending.IsUsed || pending.ExpireAt < DateTime.UtcNow)
                return View("ErrorVerification");

            var user = new User
            {
                Email = pending.Email,
                Phone = pending.Phone,
                Role = pending.Role,
                PasswordHash = pending.PasswordHash,
                EmailVerified = true,
                IsActive = true
            };

            await _userRepo.CreateAsync(user);

            if (pending.Role == "JobSeeker")
            {
                var profile = new JobSeekerProfile
                {
                    UserId = user.UserId,
                    FirstName = pending.FirstName,
                    LastName = pending.LastName,
                    LinkedIn = pending.LinkedIn,
                    GitHub = pending.GitHub,
                    Skills = pending.Skills,
                    ExperienceYears = pending.ExperienceYears,
                    ResumeFilePath = MoveResumeFile(pending.ResumeFilePath)
                };

                await _jobSeekerRepo.AddAsync(profile);
            }
            else if (pending.Role == "Employer")
            {
                var employer = new Employer
                {
                    UserId = user.UserId,
                    CompanyName = pending.CompanyName,
                    Location = pending.Location,
                    CompanySize = pending.CompanySize,
                    Website = pending.Website,
                    Industry = pending.Industry,
                    Description = pending.Description,
                    LinkedIn = pending.LinkedInCompany,
                    LogoPath = MoveLogoFile(pending.LogoPath)
                };

                await _employerRepo.AddAsync(employer);
            }

            pending.IsUsed = true;
            await _db.SaveChangesAsync();

            return View("EmailVerified");
        }


        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword([FromForm] string email)
        {
            var user = await _userRepo.GetByEmailAsync(email);

            if (user == null)
                return BadRequest(new { message = "Email not found." });

            string token = Guid.NewGuid().ToString();

            user.ResetToken = token;
            user.ResetTokenExpires = DateTime.UtcNow.AddHours(1);

            await _userRepo.UpdateAsync(user);

            string link = $"{_config["AppUrl"]}/Accounts/ResetPassword?token={token}";

            await _emailService.SendSimpleEmailAsync(
                email,
                "Reset Your Password",
                $"Click this link to reset your password:<br><br><a href='{link}'>Reset Password</a>"
            );

            return Ok(new { message = "Password reset link sent to your email." });
        }


        [HttpGet]
        public IActionResult ResetPassword(string token)
        {
            if (string.IsNullOrEmpty(token))
                return BadRequest("Invalid token.");

            return View(model: token);
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(string token, string password)
        {
            var user = await _userRepo.GetByResetTokenAsync(token);

            if (user == null || user.ResetTokenExpires < DateTime.UtcNow)
                return BadRequest(new { message = "Invalid or expired token." });

            if (password.Length < 8)
                return BadRequest(new { message = "Password must be at least 8 characters." });

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
            user.ResetToken = null;
            user.ResetTokenExpires = null;

            await _userRepo.UpdateAsync(user);

            return Ok(new { message = "Password reset successfully!" });
        }

        private string MoveResumeFile(string tempPath)
        {
            if (string.IsNullOrEmpty(tempPath)) return null;

            string relative = tempPath.TrimStart('/');
            string source = Path.Combine("wwwroot", relative);

            if (!System.IO.File.Exists(source))
                return null;

            string newDir = Path.Combine("wwwroot", "resumes");
            if (!Directory.Exists(newDir))
                Directory.CreateDirectory(newDir);

            string fileName = Path.GetFileName(relative);
            string dest = Path.Combine(newDir, fileName);

            System.IO.File.Move(source, dest);

            return "/resumes/" + fileName;
        }

        private string? MoveLogoFile(string tempPath)
        {
            if (tempPath == null) return null;

            string fileName = Path.GetFileName(tempPath);
            string newDir = Path.Combine("wwwroot", "logos");

            if (!Directory.Exists(newDir))
                Directory.CreateDirectory(newDir);

            string newPath = Path.Combine(newDir, fileName);
            System.IO.File.Move("wwwroot" + tempPath, newPath);

            return "/logos/" + fileName;
        }
    }
}
