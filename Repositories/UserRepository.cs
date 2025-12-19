using JobConnect.Data;
using JobConnect.Interfaces;
using JobConnect.Models;
using Microsoft.EntityFrameworkCore;

namespace JobConnect.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _db;

        public UserRepository(AppDbContext db)
        {
            _db = db;
        }
        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _db.Users
                .Include(u => u.JobSeekerProfile)
                .Include(u => u.Employer)
                .FirstOrDefaultAsync(u => u.Email == email);
        }
        public async Task<User?> GetByPhoneAsync(string phone)
        {
            return await _db.Users.FirstOrDefaultAsync(u => u.Phone == phone);
        }

        public async Task<User> CreateAsync(User user)
        {
            await _db.Users.AddAsync(user);
            await _db.SaveChangesAsync();
            return user;
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            return await _db.Users
                .Include(u => u.JobSeekerProfile)
                .Include(u => u.Employer)
                .FirstOrDefaultAsync(u => u.UserId == id);
        }

        public async Task UpdateAsync(User user)
        {
            _db.Users.Update(user);
            await _db.SaveChangesAsync();
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            return await _db.Users
                .OrderBy(u => u.UserId)
                .ToListAsync();
        }
        public async Task DeleteAsync(User user)
        {
            _db.Users.Remove(user);
            await _db.SaveChangesAsync();
        }

        public async Task ToggleActiveStatusAsync(int userId)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
                return;

            user.IsActive = !user.IsActive;
            await _db.SaveChangesAsync();
        }
        public async Task SaveChangesAsync()
        {
            await _db.SaveChangesAsync();
        }
        public async Task<User?> GetByResetTokenAsync(string token)
        {
            return await _db.Users
                .FirstOrDefaultAsync(u => u.ResetToken == token);
        }

    }
}
