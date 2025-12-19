using JobConnect.Models;

namespace JobConnect.Interfaces
{
    public interface IJobApplicationRepository
    {
        Task<List<JobApplication>> GetApplicationsByJobIdAsync(int jobId);
        Task<JobApplication?> GetApplicationByIdAsync(int applicationId);
        Task AddAsync(JobApplication application);
        Task DeleteAsync(JobApplication application);
        Task SaveChangesAsync();
        Task<List<JobApplication>> GetAllAsync();
        Task<List<JobApplication>> GetApplicationsByJobSeekerIdAsync(int jobSeekerId);
        Task<bool> HasAppliedAsync(int jobId, int jobSeekerId);
        Task RejectAsync(JobApplication app);


    }
}
