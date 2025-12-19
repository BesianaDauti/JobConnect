using JobConnect.Models;

namespace JobConnect.Interfaces
{
    public interface IJobSeekerRepository
    {
        Task<JobSeekerProfile?> GetJobSeekerByUserIdAsync(int userId);
        Task<JobSeekerProfile?> GetByIdAsync(int seekerId);
        Task<List<JobSeekerProfile>> GetAllAsync();
        Task AddAsync(JobSeekerProfile profile);
        Task UpdateAsync(JobSeekerProfile profile);
        Task DeleteAsync(JobSeekerProfile profile);
        Task SaveChangesAsync();

    }
}
