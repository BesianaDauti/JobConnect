using BCrypt.Net;
using JobConnect.DTOs;
using JobConnect.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace JobConnect.Controllers
{
    [NoCache]
    public class JobSeekerController : Controller
    {
        private readonly IUserRepository _userRepo;
        private readonly IJobSeekerRepository _jobSeekerRepo;
        private readonly IJobApplicationRepository _applicationRepo;
        private readonly IJobRepository _jobRepo;

        public JobSeekerController(
            IUserRepository userRepo,
            IJobSeekerRepository jobSeekerRepo,
            IJobApplicationRepository applicationRepo,
            IJobRepository jobRepo)
        {
            _userRepo = userRepo;
            _jobSeekerRepo = jobSeekerRepo;
            _applicationRepo = applicationRepo;
            _jobRepo = jobRepo;
        }

        private bool IsJobSeeker()
        {
            var id = HttpContext.Session.GetInt32("UserId");
            var role = HttpContext.Session.GetString("Role");
            return id != null && role == "JobSeeker";
        }

        private IActionResult? EnsureJobSeeker()
        {
            if (!IsJobSeeker())
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Accounts");
            }

            return null;
        }

        public async Task<IActionResult> JobSeekerProfile()
        {
            var auth = EnsureJobSeeker();
            if (auth != null) return auth;

            int userId = HttpContext.Session.GetInt32("UserId")!.Value;

            var jobseeker = await _jobSeekerRepo.GetJobSeekerByUserIdAsync(userId);

            if (jobseeker == null)
                return NotFound();

            var applications = await _applicationRepo.GetApplicationsByJobSeekerIdAsync(jobseeker.JobSeekerProfileId);

            ViewBag.Applications = applications;

            return View(jobseeker);
        }

        public async Task<IActionResult> UpdateProfile()
        {
            var auth = EnsureJobSeeker();
            if (auth != null) return auth;

            int userId = HttpContext.Session.GetInt32("UserId")!.Value;

            var jobSeeker = await _jobSeekerRepo.GetJobSeekerByUserIdAsync(userId);

            if (jobSeeker == null)
                return NotFound();

            return View(jobSeeker);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProfile([FromForm] UpdateProfileDto dto)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Unauthorized();

            var profile = await _jobSeekerRepo.GetJobSeekerByUserIdAsync(userId.Value);

            if (profile == null)
                return NotFound(new { message = "Profile not found." });

            if (!string.IsNullOrWhiteSpace(dto.FirstName))
                profile.FirstName = dto.FirstName;

            if (!string.IsNullOrWhiteSpace(dto.LastName))
                profile.LastName = dto.LastName;

            if (!string.IsNullOrWhiteSpace(dto.Email))
                profile.User.Email = dto.Email;

            if (!string.IsNullOrWhiteSpace(dto.LinkedIn))
                profile.LinkedIn = dto.LinkedIn;

            if (!string.IsNullOrWhiteSpace(dto.GitHub))
                profile.GitHub = dto.GitHub;

            if (!string.IsNullOrWhiteSpace(dto.Skills))
                profile.Skills = dto.Skills;

            if (dto.ExperienceYears != null)
                profile.ExperienceYears = dto.ExperienceYears;

            if (!string.IsNullOrWhiteSpace(dto.ProfessionalTitle))
                profile.ProfessionalTitle = dto.ProfessionalTitle;

            if (!string.IsNullOrWhiteSpace(dto.Location))
                profile.Location = dto.Location;

            if (!string.IsNullOrWhiteSpace(dto.AboutMe))
                profile.AboutMe = dto.AboutMe;

            if (!string.IsNullOrWhiteSpace(dto.University))
                profile.University = dto.University;

            if (!string.IsNullOrWhiteSpace(dto.Degree))
                profile.Degree = dto.Degree;

            if (dto.GraduationYear != null)
                profile.GraduationYear = dto.GraduationYear;

            if (!string.IsNullOrWhiteSpace(dto.Phone))
                profile.User.Phone = dto.Phone;

            if (!string.IsNullOrWhiteSpace(dto.Password))
            {
                if (dto.Password.Length < 8)
                    return BadRequest(new { message = "Password must be at least 8 characters long." });

                profile.User.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            }

            if (dto.ResumeFile != null)
            {
                var folder = Path.Combine("wwwroot", "resumes");
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

                string file = $"{Guid.NewGuid()}_{dto.ResumeFile.FileName}";
                string path = Path.Combine(folder, file);

                using var stream = new FileStream(path, FileMode.Create);
                await dto.ResumeFile.CopyToAsync(stream);

                profile.ResumeFilePath = "/resumes/" + file;
            }

            await _jobSeekerRepo.UpdateAsync(profile);
            await _jobSeekerRepo.SaveChangesAsync();

            return Ok(new
            {
                message = "Profile updated successfully!",
                redirect = Url.Action("JobSeekerProfile", "JobSeeker")
            });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateResume(IFormFile ResumeFile)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return Unauthorized(new { message = "You must be logged in." });

            var profile = await _jobSeekerRepo.GetJobSeekerByUserIdAsync(userId.Value);

            if (profile == null)
                return NotFound(new { message = "Profile not found." });

            if (ResumeFile == null || ResumeFile.Length == 0)
                return BadRequest(new { message = "Please select a valid resume file." });

            string folder = Path.Combine("wwwroot", "resumes");
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

            string fileName = $"{Guid.NewGuid()}_{ResumeFile.FileName}";
            string filePath = Path.Combine(folder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await ResumeFile.CopyToAsync(stream);
            }

            profile.ResumeFilePath = "/resumes/" + fileName;

            await _jobSeekerRepo.UpdateAsync(profile);
            await _jobSeekerRepo.SaveChangesAsync();

            return Ok(new { message = "Resume updated successfully!" });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteAccount()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Unauthorized();

            var user = await _userRepo.GetByIdAsync(userId.Value);

            if (user == null)
                return NotFound(new { message = "User not found." });

            if (user.JobSeekerProfile != null)
                await _jobSeekerRepo.DeleteAsync(user.JobSeekerProfile);

            await _userRepo.DeleteAsync(user);
            await _userRepo.SaveChangesAsync();

            HttpContext.Session.Clear();

            return Ok(new
            {
                message = "Your account has been deleted successfully."
            });
        }

        public async Task<IActionResult> Profile()
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
                return RedirectToAction("Login", "Accounts");

            var jobSeeker = await _jobSeekerRepo.GetJobSeekerByUserIdAsync(userId.Value);

            if (jobSeeker == null)
                return NotFound();

            var applications = await _applicationRepo.GetApplicationsByJobSeekerIdAsync(jobSeeker.JobSeekerProfileId);

            ViewBag.Applications = applications;

            return View("JobSeekerProfile", jobSeeker);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteApplication(int applicationId)
        {
            var app = await _applicationRepo.GetApplicationByIdAsync(applicationId);

            if (app == null)
                return Json(new { success = false, message = "Application not found." });

            await _applicationRepo.DeleteAsync(app);
            await _applicationRepo.SaveChangesAsync();

            return Json(new { success = true });

        }
    }
}
