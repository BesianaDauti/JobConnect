using BCrypt.Net;
using JobConnect.Interfaces;
using JobConnect.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace JobConnect.Controllers
{
    [NoCache]
    public class AdminController : Controller
    {
        private readonly IUserRepository _userRepo;
        private readonly IEmployerRepository _employerRepo;
        private readonly IJobSeekerRepository _jobSeekerRepo;
        private readonly IJobRepository _jobRepo;
        private readonly IJobApplicationRepository _applicationRepo;

        public AdminController(
            IUserRepository userRepo,
            IEmployerRepository employerRepo,
            IJobSeekerRepository jobSeekerRepo,
            IJobRepository jobRepo,
            IJobApplicationRepository applicationRepo)
        {
            _userRepo = userRepo;
            _employerRepo = employerRepo;
            _jobSeekerRepo = jobSeekerRepo;
            _jobRepo = jobRepo;
            _applicationRepo = applicationRepo;
        }

        private bool IsAdmin()
        {
            var id = HttpContext.Session.GetInt32("UserId");
            var role = HttpContext.Session.GetString("Role");
            return id != null && role == "Admin";
        }

        private IActionResult? EnsureAdmin()
        {
            if (!IsAdmin())
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Accounts");
            }

            return null;
        }

        public async Task<IActionResult> Dashboard()
        {
            var auth = EnsureAdmin();
            if (auth != null) return auth;

            ViewBag.TotalJobSeekers = (await _jobSeekerRepo.GetAllAsync()).Count;
            ViewBag.TotalEmployers = (await _employerRepo.GetAllAsync()).Count;
            ViewBag.TotalJobs = (await _jobRepo.GetAllJobsAsync()).Count;
            ViewBag.TotalApplications = (await _applicationRepo.GetAllAsync()).Count;

            return View();
        }

        public async Task<IActionResult> Profile()
        {
            var auth = EnsureAdmin();
            if (auth != null) return auth;

            var userId = HttpContext.Session.GetInt32("UserId")!.Value;
            var admin = await _userRepo.GetByIdAsync(userId);

            return View(admin);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProfile(int userId, string Email, string Phone, string Password)
        {
            var admin = await _userRepo.GetByIdAsync(userId);

            if (admin == null)
                return BadRequest(new { message = "Admin not found." });

            if (!string.IsNullOrWhiteSpace(Email))
                admin.Email = Email;

            if (!string.IsNullOrWhiteSpace(Phone))
                admin.Phone = Phone;

            if (!string.IsNullOrWhiteSpace(Password))
            {
                if (Password.Length < 8)
                    return BadRequest(new { message = "Password must be at least 8 characters." });

                admin.PasswordHash = BCrypt.Net.BCrypt.HashPassword(Password);
            }

            await _userRepo.UpdateAsync(admin);

            return Ok(new { message = "Profile updated successfully!" });
        }

        public async Task<IActionResult> Users()
        {
            var auth = EnsureAdmin();
            if (auth != null) return auth;

            var users = await _userRepo.GetAllUsersAsync();
            return View(users);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUser(int id)
        {
            if (!IsAdmin())
                return Json(new { success = false, message = "Unauthorized" });

            var adminId = HttpContext.Session.GetInt32("UserId");

            if (id == adminId)
                return Json(new { success = false, message = "You cannot delete your own admin account." });

            var user = await _userRepo.GetByIdAsync(id);

            if (user == null)
                return Json(new { success = false, message = "User not found." });

            await _userRepo.DeleteAsync(user);
            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> ToggleUser(int id)
        {
            if (!IsAdmin())
                return Json(new { success = false, message = "Unauthorized" });

            var adminId = HttpContext.Session.GetInt32("UserId");

            if (id == adminId)
                return Json(new { success = false, message = "You cannot disable your own admin account." });

            var user = await _userRepo.GetByIdAsync(id);

            if (user == null)
                return Json(new { success = false, message = "User not found." });

            await _userRepo.ToggleActiveStatusAsync(id);

            return Json(new { success = true });
        }

        public async Task<IActionResult> Jobs()
        {
            var auth = EnsureAdmin();
            if (auth != null) return auth;

            var jobs = await _jobRepo.GetAllJobsAsync();
            return View("AdminJobs", jobs);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteJob(int id)
        {
            if (!IsAdmin())
                return Json(new { success = false, message = "Unauthorized" });

            var job = await _jobRepo.GetJobByIdAsync(id);

            if (job == null)
                return Json(new { success = false, message = "Job not found." });

            await _jobRepo.DeleteJobAsync(job);
            await _jobRepo.SaveChangesAsync();

            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> ToggleJob(int id)
        {
            if (!IsAdmin())
                return Json(new { success = false, message = "Unauthorized" });

            var job = await _jobRepo.GetJobByIdAsync(id);

            if (job == null)
                return Json(new { success = false, message = "Job not found." });

            job.IsActive = !job.IsActive;

            await _jobRepo.UpdateJobAsync(job);
            await _jobRepo.SaveChangesAsync();

            return Json(new { success = true, newStatus = job.IsActive });
        }
    }
}
