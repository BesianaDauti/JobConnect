using JobConnect.Models;

namespace JobConnect.Interfaces
{
    public interface IJobRepository
    {
        Task<List<Job>> GetActiveJobsWithEmployerAsync();
        Task<List<Job>> GetJobsByEmployerAsync(int employerId);
        Task<Job?> GetJobByIdAsync(int jobId);
        Task<Job?> GetJobWithEmployerAsync(int jobId);
        Task AddJobAsync(Job job);
        Task UpdateJobAsync(Job job);
        Task DeleteJobAsync(Job job);
        Task<int> CountActiveJobsByEmployerAsync(int employerId);
        Task DeactivateIfFullAsync(int jobId);
        Task DeactivateExpiredJobsAsync();
        Task SaveChangesAsync();
        Task<List<Job>> GetAllJobsAsync();
    }
}
