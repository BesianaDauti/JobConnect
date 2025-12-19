using JobConnect.Data;
using JobConnect.Models;
using Microsoft.EntityFrameworkCore;

namespace JobConnect.Repositories
{
    public class EmployerRepository : IEmployerRepository
    {
        private readonly AppDbContext _db;

        public EmployerRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<Employer?> GetEmployerByUserIdAsync(int userId)
        {
            return await _db.Employers
                .Include(e => e.User)
                .FirstOrDefaultAsync(e => e.UserId == userId);
        }
        public async Task<bool> CompanyNameExistsAsync(string companyName, int? ignoreId = null)
        {
            return await _db.Employers
                .AnyAsync(e => e.CompanyName == companyName && (ignoreId == null || e.EmployerId != ignoreId));
        }

        public async Task<bool> EmailExistsAsync(string email, int? ignoreUserId = null)
        {
            return await _db.Users
                .AnyAsync(u => u.Email == email && (ignoreUserId == null || u.UserId != ignoreUserId));
        }

        public async Task<bool> PhoneExistsAsync(string phone, int? ignoreUserId = null)
        {
            return await _db.Users
                .AnyAsync(u => u.Phone == phone && (ignoreUserId == null || u.UserId != ignoreUserId));
        }

        public async Task UpdateAsync(Employer employer)
        {
            _db.Employers.Update(employer);
            await _db.SaveChangesAsync();
        }
        public async Task<List<Employer>> GetAllAsync()
        {
            return await _db.Employers
                .Include(e => e.User)
                .ToListAsync();
        }
        public async Task AddAsync(Employer employer)
        {
            await _db.Employers.AddAsync(employer);
            await _db.SaveChangesAsync();
        }

        public async Task CheckAndExpireSubscriptionAsync(int employerId)
        {
            var employer = await _db.Employers.FindAsync(employerId);
            if (employer == null) return;

            if (employer.Plan == "Paid" &&
                employer.PaidUntil.HasValue &&
                employer.PaidUntil.Value < DateTime.UtcNow)
            {
                employer.Plan = "Basic";
                employer.IsSubscriptionActive = false;
                employer.StripeSubscriptionId = null;
                employer.SubscriptionCanceledAt = null;
                employer.PaidUntil = null;

                await _db.SaveChangesAsync();
            }
        }

    }
}
