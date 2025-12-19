using JobConnect.Data;
using JobConnect.Interfaces;
using JobConnect.Models;
using Microsoft.EntityFrameworkCore;

namespace JobConnect.Repositories
{
    public class JobSeekerRepository : IJobSeekerRepository
    {
        private readonly AppDbContext _db;

        public JobSeekerRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<JobSeekerProfile?> GetJobSeekerByUserIdAsync(int userId)
        {
            return await _db.JobSeekerProfiles
                .Include(js => js.User)
                .FirstOrDefaultAsync(js => js.UserId == userId);
        }

        public async Task<JobSeekerProfile?> GetByIdAsync(int seekerId)
        {
            return await _db.JobSeekerProfiles
                .Include(js => js.User)
                .FirstOrDefaultAsync(js => js.JobSeekerProfileId == seekerId);
        }
        public async Task<List<JobSeekerProfile>> GetAllAsync()
        {
            return await _db.JobSeekerProfiles
                .Include(js => js.User)
                .ToListAsync();
        }
        public async Task AddAsync(JobSeekerProfile profile)
        {
            await _db.JobSeekerProfiles.AddAsync(profile);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(JobSeekerProfile profile)
        {
            _db.JobSeekerProfiles.Update(profile);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(JobSeekerProfile profile)
        {
            _db.JobSeekerProfiles.Remove(profile);
            await _db.SaveChangesAsync();
        }
        public async Task SaveChangesAsync()
        {
            await _db.SaveChangesAsync();
        }

    }
}
