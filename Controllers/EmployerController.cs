using BCrypt.Net;
using JobConnect.DTOs;
using JobConnect.Interfaces;
using JobConnect.Models;
using JobConnect.Repositories;
using JobConnect.Services;
using Microsoft.AspNetCore.Mvc;

namespace JobConnect.Controllers
{
    [NoCache]
    public class EmployerController : Controller
    {
        private readonly IEmployerRepository _employerRepo;
        private readonly IJobRepository _jobRepo;
        private readonly IUserRepository _userRepo;
        private readonly IJobSeekerRepository _jobSeekerRepo;
        private readonly IJobApplicationRepository _applicationRepo;
        private readonly IEmailService _emailService;

        public EmployerController(
            IEmployerRepository employerRepo,
            IJobRepository jobRepo,
            IUserRepository userRepo,
            IJobSeekerRepository jobSeekerRepo,
            IJobApplicationRepository applicationRepo,
            IEmailService emailService)
        {
            _employerRepo = employerRepo;
            _jobRepo = jobRepo;
            _userRepo = userRepo;
            _jobSeekerRepo = jobSeekerRepo;
            _applicationRepo = applicationRepo;
            _emailService = emailService;
        }

        private bool IsEmployer()
        {
            var id = HttpContext.Session.GetInt32("UserId");
            var role = HttpContext.Session.GetString("Role");
            return id != null && role == "Employer";
        }

        private IActionResult? EnsureEmployer()
        {
            if (!IsEmployer())
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Accounts");
            }

            return null;
        }

        public async Task<IActionResult> EmployerProfile()
        {
            var auth = EnsureEmployer();
            if (auth != null) return auth;

            var userId = HttpContext.Session.GetInt32("UserId")!.Value;
            var employer = await _employerRepo.GetEmployerByUserIdAsync(userId);

            return View(employer);
        }

        public async Task<IActionResult> Jobs()
        {
            var auth = EnsureEmployer();
            if (auth != null) return auth;

            var userId = HttpContext.Session.GetInt32("UserId");
            var employer = await _employerRepo.GetEmployerByUserIdAsync(userId!.Value);
            await _employerRepo.CheckAndExpireSubscriptionAsync(employer.EmployerId);
            if (employer == null)
                return NotFound();

            var jobs = await _jobRepo.GetJobsByEmployerAsync(employer.EmployerId);

            return View(jobs);
        }

        public IActionResult AddJob()
        {
            var auth = EnsureEmployer();
            if (auth != null) return auth;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddJob(JobCreateDto dto)
        {
            var auth = EnsureEmployer();
            if (auth != null) return auth;

            var userId = HttpContext.Session.GetInt32("UserId")!.Value;

            var employer = await _employerRepo.GetEmployerByUserIdAsync(userId);
            if (employer == null)
                return BadRequest(new { message = "Employer not found." });
            await _employerRepo.CheckAndExpireSubscriptionAsync(employer.EmployerId);

            bool isPaid = 
                employer.Plan == "Paid" &&
                employer.IsSubscriptionActive &&
                employer.PaidUntil >= DateTime.UtcNow;

            if (!isPaid)
            {
                int activeJobs = await _jobRepo
                    .CountActiveJobsByEmployerAsync(employer.EmployerId);

                if (activeJobs >= 3)
                {
                    return BadRequest(new
                    {
                        message = "Basic plan allows only 3 active jobs. Upgrade to Paid plan."
                    });
                }
            }
            var job = new Job
            {
                Title = dto.Title,
                CompanyName = employer.CompanyName,
                Location = dto.Location,
                JobType = dto.JobType,
                Salary = dto.Salary,
                WorkingHours = dto.WorkingHours,
                ExperienceLevel = dto.ExperienceLevel,
                Description = dto.Description,
                Deadline = dto.Deadline,
                EmployerId = employer.EmployerId,
                Requirements = dto.Requirements,
                Responsibilities = dto.Responsibilities,
                PositionsAvailable = dto.PositionsAvailable,
                PostedAt = DateTime.UtcNow
            };

            await _jobRepo.AddJobAsync(job);
            await _jobRepo.SaveChangesAsync();

            return Ok(new
            {
                message = "Job posted successfully!",
                redirect = Url.Action("Jobs", "Employer")
            });
        }

        public async Task<IActionResult> EditProfile()
        {
            var auth = EnsureEmployer();
            if (auth != null) return auth;

            var userId = HttpContext.Session.GetInt32("UserId")!.Value;
            var employer = await _employerRepo.GetEmployerByUserIdAsync(userId);

            if (employer == null)
                return NotFound();

            return View(employer);
        }

        [HttpPost]
        public async Task<IActionResult> EditProfile(Employer model, IFormFile? LogoFile, string? NewPassword)
        {
            var auth = EnsureEmployer();
            if (auth != null) return auth;

            var userId = HttpContext.Session.GetInt32("UserId")!.Value;
            var employer = await _employerRepo.GetEmployerByUserIdAsync(userId);


            if (employer == null)
                return NotFound();

            if (await _employerRepo.CompanyNameExistsAsync(model.CompanyName, employer.EmployerId))
            {
                ModelState.AddModelError("", "Company name is already taken.");
                return View(employer);
            }

            if (await _employerRepo.EmailExistsAsync(model.User.Email, employer.UserId))
            {
                ModelState.AddModelError("", "Email is already in use.");
                return View(employer);
            }

            if (!string.IsNullOrWhiteSpace(model.User.Phone))
            {
                if (await _employerRepo.PhoneExistsAsync(model.User.Phone, employer.UserId))
                {
                    ModelState.AddModelError("", "Phone is already in use.");
                    return View(employer);
                }
            }

            employer.CompanyName = model.CompanyName;
            employer.Industry = model.Industry;
            employer.Location = model.Location;
            employer.Description = model.Description;
            employer.Website = model.Website;

            employer.User.Email = model.User.Email;
            employer.User.Phone = model.User.Phone;

            if (!string.IsNullOrWhiteSpace(NewPassword))
            {
                if (NewPassword.Length < 8)
                {
                    ModelState.AddModelError("", "Password must be at least 8 characters.");
                    return View(employer);
                }

                employer.User.PasswordHash = BCrypt.Net.BCrypt.HashPassword(NewPassword);
            }

            if (LogoFile != null)
            {
                var folder = Path.Combine("wwwroot", "companylogos");
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

                var fileName = $"{Guid.NewGuid()}_{LogoFile.FileName}";
                var filePath = Path.Combine(folder, fileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                await LogoFile.CopyToAsync(stream);

                employer.LogoPath = "/companylogos/" + fileName;
            }

            await _employerRepo.UpdateAsync(employer);

            return RedirectToAction("EmployerProfile");
        }

        public async Task<IActionResult> EditJob(int id)
        {
            var auth = EnsureEmployer();
            if (auth != null) return auth;

            var job = await _jobRepo.GetJobByIdAsync(id);

            if (job == null)
                return NotFound("Job not found");

            var dto = new JobUpdateDto
            {
                JobId = job.JobId,
                Title = job.Title,
                Location = job.Location,
                JobType = job.JobType,
                Salary = job.Salary,
                WorkingHours = job.WorkingHours,
                ExperienceLevel = job.ExperienceLevel,
                Description = job.Description,
                Requirements = job.Requirements,
                Responsibilities = job.Responsibilities,
                PositionsAvailable = job.PositionsAvailable,
                Deadline = job.Deadline
            };

            return View(dto);
        }

        [HttpPost]
        public async Task<IActionResult> EditJob(JobUpdateDto dto)
        {
            var auth = EnsureEmployer();
            if (auth != null) return auth;

            var job = await _jobRepo.GetJobByIdAsync(dto.JobId);
            if (job == null)
                return BadRequest(new { success = false, message = "Job not found." });

            var userId = HttpContext.Session.GetInt32("UserId")!.Value;
            var employer = await _employerRepo.GetEmployerByUserIdAsync(userId);
            if (employer == null)
                return BadRequest(new { success = false, message = "Employer not found." });

            if (employer.Plan == "Basic" && job.Deadline < DateTime.UtcNow)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Cannot edit expired jobs on Basic plan. Upgrade to Paid plan."
                });
            }

            job.Title = dto.Title;
            job.Location = dto.Location;
            job.JobType = dto.JobType;
            job.Salary = dto.Salary;
            job.WorkingHours = dto.WorkingHours;
            job.ExperienceLevel = dto.ExperienceLevel;
            job.Description = dto.Description;
            job.Requirements = dto.Requirements;
            job.Responsibilities = dto.Responsibilities;
            job.PositionsAvailable = dto.PositionsAvailable;
            job.Deadline = dto.Deadline;

            await _jobRepo.UpdateJobAsync(job);
            await _jobRepo.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                message = "Job updated successfully!",
                redirect = Url.Action("Jobs", "Employer")
            });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteJob(int jobId)
        {
            var auth = EnsureEmployer();
            if (auth != null) return auth;

            var job = await _jobRepo.GetJobByIdAsync(jobId);

            if (job == null)
                return BadRequest(new { success = false, message = "Job not found." });

            await _jobRepo.DeleteJobAsync(job);
            await _jobRepo.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                message = "Job deleted successfully!",
                redirect = Url.Action("Jobs", "Employer")
            });
        }

        public async Task<IActionResult> Applicants(int id)
        {
            var auth = EnsureEmployer();
            if (auth != null) return auth;

            var applications = await _applicationRepo.GetApplicationsByJobIdAsync(id);

            return View(applications);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteApplicant(int applicationId)
        {
            var auth = EnsureEmployer();
            if (auth != null) return auth;

            var app = await _applicationRepo.GetApplicationByIdAsync(applicationId);

            if (app == null)
                return BadRequest(new { success = false, message = "Application not found." });

            if (app.Status == "Rejected")
                return BadRequest(new { success = false, message = "Already rejected." });

            await _applicationRepo.RejectAsync(app);
            await _applicationRepo.SaveChangesAsync();

            return Ok(new { success = true, message = "Application rejected." });
        }

        public async Task<IActionResult> ViewProfile(int id)
        {
            var auth = EnsureEmployer();
            if (auth != null) return auth;

            var profile = await _jobSeekerRepo.GetByIdAsync(id);

            if (profile == null)
                return NotFound();

            return View(profile);
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage(int jobSeekerId, string subject, string message)
        {
            var auth = EnsureEmployer();
            if (auth != null) return auth;

            var employerUserId = HttpContext.Session.GetInt32("UserId")!.Value;
            var employer = await _employerRepo.GetEmployerByUserIdAsync(employerUserId);

            if (employer == null)
                return Unauthorized();

            var jobSeeker = await _jobSeekerRepo.GetByIdAsync(jobSeekerId);

            if (jobSeeker == null)
                return NotFound();

            await _emailService.SendMailAsync(
                jobSeeker.User.Email,
                subject,
                message,
                employer.User.Email
            );
            return Ok(new { success = true, message = "Message sent!" });
        }

        [HttpPost]
        public async Task<IActionResult> AcceptApplicant(int applicationId)
        {
            var auth = EnsureEmployer();
            if (auth != null) return auth;

            var app = await _applicationRepo.GetApplicationByIdAsync(applicationId);

            if (app == null)
                return NotFound(new { success = false, message = "Application not found." });

            if (app.Status == "Accepted")
                return BadRequest(new { success = false, message = "Already accepted." });

            if (app.Job.PositionsAvailable <= 0)
                return BadRequest(new { success = false, message = "No positions left." });

            app.Status = "Accepted";
            app.Job.PositionsAvailable--;

            await _applicationRepo.SaveChangesAsync();

            await _emailService.SendMailAsync(
                app.JobSeeker.User.Email,
                "Your Job Application Was Accepted!",
                $"Hello {app.JobSeeker.FirstName},\n\n" +
                $"Congratulations! Your application for '{app.Job.Title}' was accepted.\n\n" +
                $"Best regards,\n{app.Job.Employer.CompanyName}",
                app.Job.Employer.User.Email
            );
            return Ok(new { success = true, message = "Applicant accepted and notified!" });
        }
    }
}
