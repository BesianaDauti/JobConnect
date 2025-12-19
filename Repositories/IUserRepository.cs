using JobConnect.Models;

namespace JobConnect.Interfaces
{
    public interface IUserRepository
    {
        Task<User> GetByEmailAsync(string email);
        Task<User> CreateAsync(User user);
        Task<User> GetByPhoneAsync(string phone);
        Task<User?> GetByIdAsync(int id);
        Task UpdateAsync(User user);
        Task<List<User>> GetAllUsersAsync();
        Task DeleteAsync(User user);
        Task ToggleActiveStatusAsync(int userId);
        Task SaveChangesAsync();
        Task<User?> GetByResetTokenAsync(string token);

    }
}
