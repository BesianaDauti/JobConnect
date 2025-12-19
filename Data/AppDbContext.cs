using Microsoft.EntityFrameworkCore;
using JobConnect.Models;

namespace JobConnect.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<JobSeekerProfile> JobSeekerProfiles { get; set; }
        public DbSet<Employer> Employers { get; set; }
        public DbSet<EmailToken> EmailTokens { get; set; }
        public DbSet<Job> Jobs { get; set; }
        public DbSet<JobApplication> JobApplications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasIndex(x => x.Email)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(x => x.Phone)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasOne(x => x.JobSeekerProfile)
                .WithOne(x => x.User)
                .HasForeignKey<JobSeekerProfile>(x => x.UserId);

            modelBuilder.Entity<User>()
                .HasOne(x => x.Employer)
                .WithOne(x => x.User)
                .HasForeignKey<Employer>(x => x.UserId);

            modelBuilder.Entity<Employer>()
                .HasIndex(e => e.CompanyName)
                .IsUnique();
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<JobApplication>()
                .HasOne(a => a.Job)
                .WithMany()
                .HasForeignKey(a => a.JobId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<JobApplication>()
                .HasOne(a => a.JobSeeker)
                .WithMany()
                .HasForeignKey(a => a.JobSeekerId)
                .OnDelete(DeleteBehavior.Restrict);
        }

    }
}
