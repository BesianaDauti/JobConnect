using JobConnect.Models;
namespace JobConnect.Repositories
{
    public interface IEmployerRepository
    {
        Task<Employer?> GetEmployerByUserIdAsync(int  userId);
        Task<bool> CompanyNameExistsAsync(string companyName, int? ignoreId = null);
        Task<bool> EmailExistsAsync(string email, int? ignoreUserId = null);
        Task<bool> PhoneExistsAsync(string phone, int? ignoreUserId = null);
        Task UpdateAsync(Employer employer);
        Task<List<Employer>> GetAllAsync();
        Task AddAsync(Employer employer);
        Task CheckAndExpireSubscriptionAsync(int employerId);

    }
}
