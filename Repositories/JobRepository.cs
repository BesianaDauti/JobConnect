using JobConnect.Data;
using JobConnect.Interfaces;
using JobConnect.Models;
using Microsoft.EntityFrameworkCore;

namespace JobConnect.Repositories
{
    public class JobRepository : IJobRepository
    {
        private readonly AppDbContext _db;

        public JobRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<List<Job>> GetActiveJobsWithEmployerAsync()
        {
            var today = DateTime.UtcNow.Date;

            return await _db.Jobs
                .Include(j => j.Employer)
                .Where(j =>
                    j.IsActive &&
                    j.PositionsAvailable > 0 &&
                    j.Deadline.Date >= today)
                .OrderByDescending(j => j.PostedAt)
                .ToListAsync();
        }

        public async Task<List<Job>> GetJobsByEmployerAsync(int employerId)
        {
            return await _db.Jobs
                .Where(j => j.EmployerId == employerId)
                .OrderByDescending(j => j.PostedAt)
                .ToListAsync();
        }

        public async Task<Job?> GetJobByIdAsync(int jobId)
        {
            return await _db.Jobs.FirstOrDefaultAsync(j => j.JobId == jobId);
        }

        public async Task<Job?> GetJobWithEmployerAsync(int jobId)
        {
            return await _db.Jobs
                .Include(j => j.Employer)
                .FirstOrDefaultAsync(j => j.JobId == jobId);
        }

        public async Task AddJobAsync(Job job)
        {
            await _db.Jobs.AddAsync(job);
        }

        public async Task UpdateJobAsync(Job job)
        {
            _db.Jobs.Update(job);
        }

        public async Task DeleteJobAsync(Job job)
        {
            _db.Jobs.Remove(job);
        }

        public async Task<int> CountActiveJobsByEmployerAsync(int employerId)
        {
            var today = DateTime.UtcNow.Date;

            return await _db.Jobs.CountAsync(j =>
                j.EmployerId == employerId &&
                j.IsActive &&
                j.PositionsAvailable > 0 &&
                j.Deadline.Date >= today
            );
        }

        public async Task DeactivateIfFullAsync(int jobId)
        {
            var job = await _db.Jobs.FirstOrDefaultAsync(j => j.JobId == jobId);

            if (job != null && job.PositionsAvailable <= 0)
                job.IsActive = false;

            await _db.SaveChangesAsync();
        }

        public async Task DeactivateExpiredJobsAsync()
        {
            var today = DateTime.UtcNow.Date;

            var expiredJobs = await _db.Jobs
                .Where(j => j.IsActive && j.Deadline.Date < today)
                .ToListAsync();

            foreach (var job in expiredJobs)
                job.IsActive = false;

            await _db.SaveChangesAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _db.SaveChangesAsync();
        }

        public async Task<List<Job>> GetAllJobsAsync()
        {
            return await _db.Jobs
                .Include(j => j.Employer)
                .OrderByDescending(j => j.PostedAt)
                .ToListAsync();
        }

    }
}
