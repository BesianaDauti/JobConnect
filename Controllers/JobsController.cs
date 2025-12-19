using JobConnect.Interfaces;
using JobConnect.Models;
using Microsoft.AspNetCore.Mvc;

namespace JobConnect.Controllers
{
    [NoCache]
    public class JobsController : Controller
    {
        private readonly IJobRepository _jobRepo;
        private readonly IJobSeekerRepository _jobSeekerRepo;
        private readonly IJobApplicationRepository _applicationRepo;

        public JobsController(
            IJobRepository jobRepo,
            IJobSeekerRepository jobSeekerRepo,
            IJobApplicationRepository applicationRepo)
        {
            _jobRepo = jobRepo;
            _jobSeekerRepo = jobSeekerRepo;
            _applicationRepo = applicationRepo;
        }

        public async Task<IActionResult> Jobs(string? category, string? jobType, string? location)
        {
            await _jobRepo.DeactivateExpiredJobsAsync();

            var jobs = await _jobRepo.GetActiveJobsWithEmployerAsync();

            if (!string.IsNullOrWhiteSpace(category) && category != "All")
                jobs = jobs.Where(j => j.Title == category).ToList();

            if (!string.IsNullOrWhiteSpace(jobType) && jobType != "All")
                jobs = jobs.Where(j => j.JobType == jobType).ToList();

            if (!string.IsNullOrWhiteSpace(location) && location != "All")
            {
                if (location == "Remote")
                    jobs = jobs.Where(j => j.Location.ToLower() == "remote").ToList();
                else if (location == "Onsite")
                    jobs = jobs.Where(j => j.Location.ToLower() != "remote").ToList();
            }

            ViewBag.Categories = jobs.Select(j => j.Title).Distinct().ToList();
            ViewBag.JobTypes = jobs.Select(j => j.JobType).Distinct().ToList();
            ViewBag.Locations = jobs.Select(j => j.Location).Distinct().ToList();

            return View(jobs);
        }

        public async Task<IActionResult> Details(int id)
        {
            var job = await _jobRepo.GetJobWithEmployerAsync(id);

            if (job == null || !job.IsActive)
                return NotFound();

            return View(job);
        }

        [HttpGet]
        public IActionResult ApplyCheck(int jobId)
        {
            var id = HttpContext.Session.GetInt32("UserId");
            var role = HttpContext.Session.GetString("Role");

            if (id == null || role != "JobSeeker")
                return Unauthorized(new
                {
                    isJobSeeker = false,
                    message = "Login required"
                });

            return Ok(new
            {
                isJobSeeker = true
            });
        }
        [HttpPost]
        public async Task<IActionResult> Apply(int jobId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var role = HttpContext.Session.GetString("Role");

            if (userId == null || role != "JobSeeker")
                return Unauthorized(new
                {
                    success = false,
                    message = "You must be logged in as a Job Seeker."
                });

            var jobSeeker = await _jobSeekerRepo.GetJobSeekerByUserIdAsync(userId.Value);
            if (jobSeeker == null)
                return BadRequest(new
                {
                    success = false,
                    message = "JobSeeker profile not found."
                });

            var job = await _jobRepo.GetJobByIdAsync(jobId);
            if (job == null || !job.IsActive || job.Deadline < DateTime.UtcNow)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "This job has expired."
                });
            }

            bool alreadyApplied = await _applicationRepo
                .HasAppliedAsync(jobId, jobSeeker.JobSeekerProfileId);

            if (alreadyApplied)
                return BadRequest(new
                {
                    success = false,
                    message = "You have already applied for this job."
                });

            var application = new JobApplication
            {
                JobId = jobId,
                JobSeekerId = jobSeeker.JobSeekerProfileId,
                AppliedAt = DateTime.UtcNow,
                Status = "Pending"
            };

            await _applicationRepo.AddAsync(application);
            await _applicationRepo.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                message = "Your application has been submitted successfully!"
            });
        }
    }
}
