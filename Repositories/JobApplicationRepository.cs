using JobConnect.Data;
using JobConnect.Interfaces;
using JobConnect.Models;
using Microsoft.EntityFrameworkCore;

namespace JobConnect.Repositories
{
    public class JobApplicationRepository : IJobApplicationRepository
    {
        private readonly AppDbContext _db;

        public JobApplicationRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<List<JobApplication>> GetApplicationsByJobIdAsync(int jobId)
        {
            return await _db.JobApplications
                .Where(a => a.JobId == jobId)
                .Include(a => a.JobSeeker)
                    .ThenInclude(js => js.User)
                .ToListAsync();
        }

        public async Task<JobApplication?> GetApplicationByIdAsync(int id)
        {
            return await _db.JobApplications
                .Include(a => a.JobSeeker)
                    .ThenInclude(js => js.User)
                .Include(a => a.Job)
                    .ThenInclude(j => j.Employer)
                        .ThenInclude(e => e.User)
                .FirstOrDefaultAsync(a => a.ApplicationId == id);
        }

        public async Task AddAsync(JobApplication app)
        {
            await _db.JobApplications.AddAsync(app);
        }

        public async Task DeleteAsync(JobApplication app)
        {
            _db.JobApplications.Remove(app);
        }

        public async Task RejectAsync(JobApplication app)
        {
            app.Status = "Rejected";
            _db.JobApplications.Update(app);
        }

        public async Task SaveChangesAsync()
        {
            await _db.SaveChangesAsync();
        }
        public async Task<List<JobApplication>> GetAllAsync()
        {
            return await _db.JobApplications
                .Include(a => a.JobSeeker)
                    .ThenInclude(js => js.User)
                .Include(a => a.Job)
                .ToListAsync();
        }
        public async Task<List<JobApplication>> GetApplicationsByJobSeekerIdAsync(int jobSeekerId)
        {
            return await _db.JobApplications
                .Where(a => a.JobSeekerId == jobSeekerId)
                .Include(a => a.Job)
                .ToListAsync();
        }
        public async Task<bool> HasAppliedAsync(int jobId, int jobSeekerId)
        {
            return await _db.JobApplications
                .AnyAsync(a => a.JobId == jobId && a.JobSeekerId == jobSeekerId);
        }

    }
}
