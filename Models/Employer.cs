using System.Collections.Generic;
namespace JobConnect.Models
{
    public class Employer
    {
        public int EmployerId { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }
        public string CompanyName { get; set; }
        public string Location { get; set; }
        public string CompanySize { get; set; }
        public string? Website { get; set; }
        public string? Industry { get; set; }
        public string? Description { get; set; }
        public string? LinkedIn { get; set; }
        public string? LogoPath { get; set; }

        public List<Job> Jobs { get; set; }
        public string Plan { get; set; } = "Basic";
        public DateTime? PaidUntil { get; set; }  
        public bool IsSubscriptionActive { get; set; } = false;
        public string? StripeCustomerId { get; set; }
        public string? StripeSubscriptionId { get; set; }
        public DateTime? SubscriptionCanceledAt { get; set; }

    }
}
